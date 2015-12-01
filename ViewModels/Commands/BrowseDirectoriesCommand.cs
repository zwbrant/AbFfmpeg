using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AbFfmpeg.Commands
{
    public class BrowseDirectoriesCommand : ICommand
    {
        public VidConverterViewModel ViewModel { get; set; }

        public BrowseDirectoriesCommand(VidConverterViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            ViewModel.OpenDirectoryBrowser();
        }

        public event EventHandler CanExecuteChanged;
    }
}
