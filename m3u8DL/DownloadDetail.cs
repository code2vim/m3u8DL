using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace m3u8DL
{
    internal class DownloadDetail : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _file_name;
        private string _file_path;
        private string _file_duration;
        private string _progress_desc;
        //private ObservableCollection<Piece> _piece_items;

        public string FileName
        {
            get { return _file_name; }
            set
            {
                _file_name = value;
                if (PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("FileName"));
                }
            }
        }

        public string FilePath
        {
            get { return _file_path; }
            set
            {
                _file_path = value;
                if (PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("FilePath"));
                }
            }
        }

        public string FileDuration
        {
            get { return _file_duration; }
            set
            {
                _file_duration = value;
                if (PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("FileDuration"));
                }
            }
        }

        public string ProgressDesc
        {
            get { return _progress_desc; }
            set
            {
                _progress_desc = value;
                if (PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ProgressDesc"));
                }
            }
        }

        //public ObservableCollection<Piece> PieceItems
        //{
        //    get { return _piece_items; }
        //    set
        //    {
        //        _piece_items = value;
        //        if (PropertyChanged != null)
        //        {
        //            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("PieceItems"));
        //        }
        //    }
        //}
    }
}
