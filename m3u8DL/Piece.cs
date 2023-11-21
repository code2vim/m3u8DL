using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m3u8DL
{
    internal class Piece : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private int _status;
        private string _color;
        public int Status
        {
            get { return _status; }
            set
            {
                _status = value;
                if (PropertyChanged != null)
                {
                    this.Color = colorTransition(_status);
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Status"));
                }
            }
        }
        public string Color
        {
            get { return _color; }
            set
            {
                _color = value;
                if (PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Color"));
                }
            }
        }

        private string colorTransition(int status)
        {
            switch (_status)
            {
                case 0:
                    return "#ebedf0";
                case 1:
                    return "#40c463";
                case 2:
                    return "#39d353";
                default:
                    return "#fff";
            }
        }
    }
}
