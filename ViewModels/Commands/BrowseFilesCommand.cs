using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AbFfmpeg.Commands
{
    public class BrowseFilesCommand : ICommand
    {
        public VidConverterViewModel ViewModel { get; set; }

        public BrowseFilesCommand(VidConverterViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            ViewModel.OpenFileBrowser();
        }

        public event EventHandler CanExecuteChanged;
    }
}
