using PropertyChanged;
using System.ComponentModel;

namespace Mitsubishi_Test
{
    [AddINotifyPropertyChangedInterface]
    public class BaseViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
    }
}
