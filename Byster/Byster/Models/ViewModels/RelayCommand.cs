using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Byster.Models.ViewModels
{
    public class RelayCommand : ICommand
    {
        private Action<object> _execute;
        private Func<object, bool> _canExecute;
        private Action _executeWithoutParam;
        private Func<bool> _canExecuteWithoutParam;

        public void Execute(object parameter)
        {
            if (_execute != null)
                _execute(parameter);
            else if (_executeWithoutParam != null)
                _executeWithoutParam();
        }
        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
                return _canExecute(parameter);
            else if (_canExecuteWithoutParam != null)
                return _canExecuteWithoutParam();
            else
                return true;
        }

        public RelayCommand(Action<object> executeDel, Func<object, bool> canExecuteDel = null)
        {
            _execute = executeDel;
            _canExecute = canExecuteDel;
        }

        public RelayCommand(Action executeDel, Func<bool> canExecuteDel = null)
        {
            _executeWithoutParam = executeDel;
            _canExecuteWithoutParam = canExecuteDel;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
