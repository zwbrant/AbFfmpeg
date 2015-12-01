using AbFfmpeg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AbFfmpeg.Commands
{
    public class ConvertCommand : ICommand
    {
        public VidConverterViewModel ViewModel { get; set; }

        public ConvertCommand(VidConverterViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            ViewModel.ConvertVideo();
        }

        public event EventHandler CanExecuteChanged;
    }
}
