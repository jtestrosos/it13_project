namespace Medicine_ERP_Desktop
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage())
            {
                Width = 1400,
                Height = 900,
                MinimumWidth = 1200,
                MinimumHeight = 700,
                Title = "PharmERP - Pharmacy Management System"
            };
        }
    }
}