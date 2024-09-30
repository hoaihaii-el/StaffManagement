﻿namespace StaffManagmentNET.ViewModels
{
    public class ChangeTimeRequestVM
    {
        public string? StaffID { get; set; }
        public string? Date { get; set; }
        public int H1 { get; set; }
        public int M1 { get; set; }
        public int H2 { get; set; }
        public int M2 { get; set; }
        public string? WrkType { get; set; }
        public string? Off { get; set; }
        public string? Reason { get; set; }
        public string? Evidence { get; set; }
    }
}
