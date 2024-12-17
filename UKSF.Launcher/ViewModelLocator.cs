using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using UKSF.Launcher.Services;
using UKSF.Launcher.ViewModels;

namespace UKSF.Launcher {
    public class ViewModelLocator {
        public ViewModelLocator() {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            
            // Services
            SimpleIoc.Default.Register<IStartupService, StartupService>();
            
            // ViewModels
            SimpleIoc.Default.Register<MainViewModel>();
        }

        public MainViewModel MainViewModel => ServiceLocator.Current.GetInstance<MainViewModel>();
    }
}
