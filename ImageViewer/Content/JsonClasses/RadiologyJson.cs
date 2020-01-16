using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.Content.JsonClasses {

    public class ImageRecord {
        public string instanceNumber {
            get; set;
        }
        public string url {
            get; set;
        }
    }

    public class SeriesRecord {
        public List<ImageRecord> imageRecords {
            get; set;
        }
        public string seriesInstanceUID {
            get; set;
        }
    }

    public class StudyRecord {
        public List<SeriesRecord> seriesRecords {
            get; set;
        }
        public string studyInstanceUID {
            get; set;
        }
    }

    public class RadiologyJson {
        public string patientId {
            get; set;
        }
        public string patientName {
            get; set;
        }
        public List<StudyRecord> studyRecords {
            get; set;
        }
    }
}
