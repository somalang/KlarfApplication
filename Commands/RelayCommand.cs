using System;
using System.Windows.Input;

namespace KlarfApplication
{
    /// <summary>
    /// ICommand를 간단히 구현하기 위한 공용 명령 클래스.
    /// ⭐️ [수정] 파라미터가 없는 기존 생성자와 파라미터를 받는 새 생성자를 모두 지원합니다.
    /// </summary>
    public class RelayCommand : ICommand
    {
        // ⭐️ [수정] 두 가지 타입의 Action/Predicate를 저장할 필드
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        private readonly Action<object> _executeWithParam;
        private readonly Predicate<object> _canExecuteWithParam;

        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// ⭐️ [기존] 파라미터가 없는 Action을 위한 생성자
        /// </summary>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// ⭐️ [추가] 파라미터를 받는 Action<object>를 위한 생성자
        /// </summary>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _executeWithParam = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecuteWithParam = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            // ⭐️ [수정] 두 가지 타입의 canExecute를 모두 확인
            if (_canExecute != null)
                return _canExecute();
            if (_canExecuteWithParam != null)
                return _canExecuteWithParam(parameter);

            return true; // canExecute가 지정되지 않으면 항상 true
        }

        public void Execute(object parameter)
        {
            // ⭐️ [수정] 두 가지 타입의 execute를 모두 확인
            _execute?.Invoke();
            _executeWithParam?.Invoke(parameter);
        }

        /// <summary>
        /// ⭐️ [유지] DefectInfoViewModel이 사용하는 RaiseCanExecuteChanged() 메서드
        /// </summary>
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}