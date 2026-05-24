// Resolve ambiguity between System.Windows.Application (WPF) and
// System.Windows.Forms.Application (WinForms) introduced by UseWindowsForms=true.
// All code in this project uses WPF's Application unless explicitly qualified.
global using Application = System.Windows.Application;
