using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlarfApplication.Model
{
    // 모델이 직접 뷰에 바인딩되어 동적으로 UI 갱신하는 경우는 모델베이스에 넣기
    public class ModelBase
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
