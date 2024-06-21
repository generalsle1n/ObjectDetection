using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

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
            Mat Matrix = CvInvoke.Imread(Result.FullPath, Emgu.CV.CvEnum.ImreadModes.Color);
            //Convert Matrix into Source FileType
            byte[] ResultData = CvInvoke.Imencode($".{(Result.ContentType.Split(_splitContentType))[1]}", Matrix);
            //Create Memory Stream for ImageSource
            MemoryStream MemoryStream = new MemoryStream(ResultData);
            //Create the Source to be loadable into Maui
            ImageSource Source = ImageSource.FromStream(() => MemoryStream);
            //Load the source
            ImageBox.Source = Source;
           
        }
        private async void OnToggleAsync(object sender, EventArgs e)
        {
            ToggledEventArgs Event = (ToggledEventArgs)e;
            if (Event.Value == true)
            {
                LoadImageButton.IsEnabled = false;
            }
            else if (Event.Value == false)
            {
                LoadImageButton.IsEnabled = true;
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
