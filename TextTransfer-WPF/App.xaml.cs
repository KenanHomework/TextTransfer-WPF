using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TextTransfer_WPF.MVVM.ViewModels;

namespace TextTransfer_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Members

        public static Container Container { get; set; } = new Container();

        public static string SuccesSoundEffect = "https://res.cloudinary.com/kysbv/video/upload/v1661935108/WolfTaxi/success-sound-effect.mp3";
        public static string ErrorSoundEffect = "https://res.cloudinary.com/kysbv/video/upload/v1661936264/WolfTaxi/error-sound.mp3";
        public static string NotificationSoundEffect = "https://res.cloudinary.com/kysbv/video/upload/v1661940169/WolfTaxi/notification-sound.mp3";

        #endregion

        #region Methods

        public void Register()
        {
            Container.RegisterSingleton<TransferWindowVM>();


            Container.Verify();
        }

        #endregion

        public App()
        {
            Register();
        }
    }
}
