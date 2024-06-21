using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Text;
using Point = System.Drawing.Point;

namespace ObjectDetection
{
    public partial class MainPage : ContentPage
    {
        private readonly PickOptions _pickOptions = new PickOptions()
        {
            PickerTitle = AppInfo.Name,
            FileTypes = FilePickerFileType.Images
        };

        private const string _splitContentType = "/";
        private List<CascadeClassifier> _classifiers = new List<CascadeClassifier>();

        public MainPage()
        {
            InitializeComponent();
            Array Values = Enum.GetValues(typeof(DetectionType));
            foreach (DetectionType Type in Values)
            {
                TypePicker.Items.Add(Enum.GetName(Type.GetType(), Type));
        }

            TypePicker.SelectedIndex = 0;
        }

        private async void OnLoadClickAsync(object sender, EventArgs e)
        {
            //File Dialog
            FileResult Result = await FilePicker.PickAsync(_pickOptions);
            //ReadFile into Matrix
            Mat Matrix = CvInvoke.Imread(Result.FullPath, ImreadModes.Color);
            //Try to Detect
            Matrix = await DetectCascade(Matrix);
            //Load the source
            ImageBox.Source = await ConvertMatToImage(Matrix);
        }
        private async Task<ImageSource> ConvertMatToImage(Mat Matrix)
        {
            //Convert Matrix into Source FileType
            byte[] ResultData = CvInvoke.Imencode($".png", Matrix);
            //Create Memory Stream for ImageSource
            MemoryStream MemoryStream = new MemoryStream(ResultData);
            //Create the Source to be loadable into Maui
            ImageSource Source = ImageSource.FromStream(() => MemoryStream);
           
            return Source;
        }

        private async Task<Mat> DetectCascade(Mat Matrix)
        {
            Mat GreyMat = new Mat();
            CvInvoke.CvtColor(Matrix, GreyMat, ColorConversion.Bgr2Gray);
            foreach (CascadeClassifier Classifier in _classifiers)
            {
                Rectangle[] Search = Classifier.DetectMultiScale(GreyMat);
                if(Search.Length != 0)
                {
                    Matrix = await DrawRectangle(Matrix, Search);
                }
            }
            return Matrix;
        }
        private async Task<Mat> DrawRectangle(Mat Matrix, Rectangle[] Rectangle)
        {
            foreach(Rectangle Single in Rectangle)
            {
                CvInvoke.Rectangle(Matrix, Single, new MCvScalar());
            }
            return Matrix;
        }

        private async void OnToggleAsync(object sender, EventArgs e)
        {
            ToggledEventArgs Event = (ToggledEventArgs)e;
            if (Event.Value == true)
            {
                LoadImageButton.IsEnabled = false;
                await CaptureVideo();
            }
            else if (Event.Value == false)
            {
                LoadImageButton.IsEnabled = true;
            }
        }
        private async Task CaptureVideo()
        {
            using (VideoCapture Capture = new VideoCapture(1))
            {
                while (true)
                {
                    Mat Picture = Capture.QueryFrame();
                    ImageBox.Source = await ConvertMatToImage(Picture);
                }
            }
        }
        private async void OnSelectChangeAsync(object sender, EventArgs e)
        {
            _classifiers.ForEach(item => item.Dispose());
            _classifiers.Clear();

            Assembly Assembly = Assembly.GetExecutingAssembly();
            string[] ResouceName = Assembly.GetManifestResourceNames();

            Picker Picker = (Picker)sender;
            string[] AllCascades = ResouceName.Where(i => i.Contains((string)Picker.SelectedItem)).ToArray();

            foreach (string Cascades in AllCascades)
            {
                using (Stream MemoryStream = Assembly.GetManifestResourceStream(Cascades))
                {
                    byte[] Data = new byte[MemoryStream.Length];
                    await MemoryStream.ReadAsync(Data);
                    string EncodedData = Encoding.UTF8.GetString(Data);

                    FileStorage Storage = new FileStorage(EncodedData, FileStorage.Mode.Memory);
                    FileNode Node = Storage.GetFirstTopLevelNode();

                    CascadeClassifier Classifier = new CascadeClassifier();
                    Classifier.Read(Node);
            
                    _classifiers.Add(Classifier);
                }
            }
        }
    }

}
