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

        public MainPage()
        {
            InitializeComponent();
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
    }

}
