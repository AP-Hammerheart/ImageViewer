using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.Content.JsonClasses {

    public class Dicom {
        public string DICOMPatientRecord {
            get; set;
        }
        public string DICOMStudyRecord {
            get; set;
        }
        public string DICOMSeriesRecord {
            get; set;
        }
        public string DICOMImageRecord {
            get; set;
        }
        public string imageSource {
            get; set;
        }
        public int imageIndexStart {
            get; set;
        }
        public int imageIndexEnd {
            get; set;
        }
        public double P1x {
            get; set;
        }
        public double P1y {
            get; set;
        }
        public double P2x {
            get; set;
        }
        public double P2y {
            get; set;
        }
        public double P3x {
            get; set;
        }
        public double P3y {
            get; set;
        }
        public double P4x {
            get; set;
        }
        public double P4y {
            get; set;
        }
    }

    public class Macro {
        public string label {
            get; set;
        }
        public string imageSource {
            get; set;
        }
        public int imageZoomLevel {
            get; set;
        }
        public double P1x {
            get; set;
        }
        public double P1y {
            get; set;
        }
        public double P2x {
            get; set;
        }
        public double P2y {
            get; set;
        }
        public double P3x {
            get; set;
        }
        public double P3y {
            get; set;
        }
        public double P4x {
            get; set;
        }
        public double P4y {
            get; set;
        }
    }

    public class Histology {
        public string imageSource {
            get; set;
        }
    }

    public class Image {
        public string label {
            get; set;
        }
        public List<Dicom> dicom {
            get; set;
        }
        public List<Macro> macro {
            get; set;
        }
        public List<Histology> histology {
            get; set;
        }
    }

    public class Item {
        public List<Image> Images {
            get; set;
        }
    }

    public class ImageConnections {
        public List<Item> Items {
            get; set;
        }
    }
}
