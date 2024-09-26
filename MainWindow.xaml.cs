using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.IO;

namespace WpfGroupProjectFinal
{
    public partial class MainWindow : Window
    {
        string xmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data\XMLFile1.xml");
        public ObservableCollection<Appointment> Appointments { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Appointments = new ObservableCollection<Appointment>();
            lstAppointments.ItemsSource = Appointments;
            LoadAppointmentsFromXML();
        }

        private void LoadAppointmentsFromXML()
        {
            Appointments.Clear(); // Clear existing entries to avoid duplication
            XDocument? doc = XDocument.Load(xmlFilePath);
            if (doc != null)
            {
                foreach (var el in doc.Descendants("Appointment"))
                {
                    Appointments.Add(new Appointment
                    {
                        AppointmentID = Convert.ToInt32(el.Element("AppointmentID")?.Value ?? ""),
                        Name = el.Element("Name")?.Value ?? "",
                        Age = Convert.ToInt32(el.Element("Age")?.Value ?? ""),
                        AppointmentType = el.Element("AppointmentType")?.Value ?? "",
                        DayOfAppointment = el.Element("DayOfAppointment")?.Value ?? "",
                        TimeOfAppointment = el.Element("TimeOfAppointment")?.Value ?? ""
                    });
                }
            }
        }

        private void AddAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields(out string? message))
            {
                MessageBox.Show(message, "Validation Error");
                return;
            }

            try
            {
                var newAppointment = new Appointment
                {
                    AppointmentID = int.Parse(txtAppointmentID.Text),
                    Name = txtName.Text ?? "",
                    Age = int.Parse(txtAge.Text),
                    AppointmentType = ((ComboBoxItem?)cmbAppointmentType.SelectedItem)?.Content?.ToString() ?? "",
                    DayOfAppointment = ((ComboBoxItem?)cmbDayOfAppointment.SelectedItem)?.Content?.ToString() ?? "",
                    TimeOfAppointment = ((ComboBoxItem?)cmbTimeOfAppointment.SelectedItem)?.Content?.ToString() ?? ""
                };

                if (IsAppointmentIDExists(newAppointment.AppointmentID))
                {
                    MessageBox.Show("Appointment ID already exists. Please use a different ID.", "Error");
                    return;
                }

                SaveAppointmentToXML(newAppointment);
                LoadAppointmentsFromXML();  // Refresh the list in the UI

                MessageBox.Show("Appointment added successfully.");
                ClearAllEntries();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error");
            }
        }

        private void SaveAppointmentToXML(Appointment appointment)
        {
            XDocument? doc = XDocument.Load(xmlFilePath);
            if (doc != null)
            {
                doc.Root?.Add(new XElement("Appointment",
                    new XElement("AppointmentID", appointment.AppointmentID),
                    new XElement("Name", appointment.Name),
                    new XElement("Age", appointment.Age),
                    new XElement("AppointmentType", appointment.AppointmentType),
                    new XElement("DayOfAppointment", appointment.DayOfAppointment),
                    new XElement("TimeOfAppointment", appointment.TimeOfAppointment)
                ));
                doc.Save(xmlFilePath);
            }
        }

        private void UpdateAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields(out string? message))
            {
                MessageBox.Show(message, "Validation Error");
                return;
            }

            try
            {
                int appointmentID = int.Parse(txtAppointmentID.Text);
                if (!IsAppointmentIDExists(appointmentID))
                {
                    MessageBox.Show("No appointment found with this ID.", "Error");
                    return;
                }

                XDocument? doc = XDocument.Load(xmlFilePath);
                if (doc != null)
                {
                    var existingAppointment = doc.Descendants("Appointment").FirstOrDefault(appt => int.Parse(appt.Element("AppointmentID")?.Value ?? "") == appointmentID);
                    if (existingAppointment != null)
                    {
                        existingAppointment.SetElementValue("Name", txtName.Text ?? "");
                        existingAppointment.SetElementValue("Age", txtAge.Text);
                        existingAppointment.SetElementValue("AppointmentType", ((ComboBoxItem?)cmbAppointmentType.SelectedItem)?.Content?.ToString() ?? "");
                        existingAppointment.SetElementValue("DayOfAppointment", ((ComboBoxItem?)cmbDayOfAppointment.SelectedItem)?.Content?.ToString() ?? "");
                        existingAppointment.SetElementValue("TimeOfAppointment", ((ComboBoxItem?)cmbTimeOfAppointment.SelectedItem)?.Content?.ToString() ?? "");

                        doc.Save(xmlFilePath);
                        LoadAppointmentsFromXML();  // Refresh the list

                        MessageBox.Show("Appointment updated successfully.");
                        ClearAllEntries();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error");
            }
        }

        private void DeleteAppointment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int appointmentID = int.Parse(txtAppointmentID.Text);
                if (!IsAppointmentIDExists(appointmentID))
                {
                    MessageBox.Show("No appointment found with this ID to delete.", "Error");
                    return;
                }

                XDocument? doc = XDocument.Load(xmlFilePath);
                if (doc != null)
                {
                    var appointment = doc.Descendants("Appointment").FirstOrDefault(appt => int.Parse(appt.Element("AppointmentID")?.Value ?? "") == appointmentID);
                    if (appointment != null)
                    {
                        appointment.Remove();
                        doc.Save(xmlFilePath);
                        LoadAppointmentsFromXML();  // Refresh the list

                        MessageBox.Show("Appointment deleted successfully.");
                        ClearAllEntries();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error");
            }
        }

        private void SearchByName_Click(object sender, RoutedEventArgs e)
        {
            ResetListViewItemHighlighting();
            string searchName = txtSearchName.Text.Trim();

            if (string.IsNullOrEmpty(searchName))
            {
                MessageBox.Show("Please enter a name to search.", "Search Error");
                return;
            }

            var foundItems = Appointments.Where(a => a.Name.Equals(searchName, StringComparison.OrdinalIgnoreCase)).ToList();

            if (foundItems.Any())
            {
                foreach (var foundItem in foundItems)
                {
                    ListViewItem? listViewItem = lstAppointments.ItemContainerGenerator.ContainerFromItem(foundItem) as ListViewItem;
                    if (listViewItem != null)
                    {
                        listViewItem.Background = Brushes.LightGreen;
                    }
                }
                MessageBox.Show("Appointments with the specified name found and highlighted.", "Search Success");
            }
            else
            {
                MessageBox.Show("No appointments found with the specified name.", "Search Result");
            }
            txtSearchName.Clear();  // Clear search field after searching
        }

        private void ClearAllEntries()
        {
            txtAppointmentID.Clear();
            txtName.Clear();
            txtAge.Clear();
            cmbAppointmentType.SelectedIndex = -1;
            cmbDayOfAppointment.SelectedIndex = -1;
            cmbTimeOfAppointment.SelectedIndex = -1;
            txtSearchName.Clear();
        }

        private void ResetListViewItemHighlighting()
        {
            foreach (Appointment appointment in lstAppointments.Items)
            {
                ListViewItem? listViewItem = lstAppointments.ItemContainerGenerator.ContainerFromItem(appointment) as ListViewItem;
                if (listViewItem != null)
                {
                    listViewItem.Background = Brushes.Transparent;
                }
            }
        }

        private bool IsAppointmentIDExists(int appointmentID)
        {
            XDocument? doc = XDocument.Load(xmlFilePath);
            return doc != null && doc.Descendants("Appointment").Any(appt => int.Parse(appt.Element("AppointmentID")?.Value ?? "") == appointmentID);
        }

        private bool ValidateFields(out string? message)
        {
            if (!Regex.IsMatch(txtAppointmentID.Text, @"^\d+$"))
            {
                message = "Appointment ID must be numeric.";
                return false;
            }
            if (!Regex.IsMatch(txtName.Text ?? "", @"^[a-zA-Z\s]+$"))
            {
                message = "Name must contain only letters and spaces.";
                return false;
            }
            if (!Regex.IsMatch(txtAge.Text, @"^\d+$"))
            {
                message = "Age must be numeric.";
                return false;
            }
            message = "";
            return true;
        }
    }

    public class Appointment
    {
        public int AppointmentID { get; set; }
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string AppointmentType { get; set; } = "";
        public string DayOfAppointment { get; set; } = "";
        public string TimeOfAppointment { get; set; } = "";
    }
}
