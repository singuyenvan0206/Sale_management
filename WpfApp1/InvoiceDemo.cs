using System;
using System.Collections.Generic;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Demo class to test the invoice print functionality
    /// </summary>
    public static class InvoiceDemo
    {
        public static void ShowDemoInvoice()
        {
            // Create sample invoice items
            var items = new List<InvoiceItemViewModel>
            {
                new InvoiceItemViewModel
                {
                    ProductId = 1,
                    ProductName = "Laptop Dell XPS 13",
                    UnitPrice = 15000000,
                    Quantity = 1,
                    LineTotal = 15000000
                },
                new InvoiceItemViewModel
                {
                    ProductId = 2,
                    ProductName = "Wireless Mouse Logitech",
                    UnitPrice = 500000,
                    Quantity = 2,
                    LineTotal = 1000000
                },
                new InvoiceItemViewModel
                {
                    ProductId = 3,
                    ProductName = "USB-C Hub",
                    UnitPrice = 800000,
                    Quantity = 1,
                    LineTotal = 800000
                }
            };

            // Create sample customer
            var customer = new CustomerListItem
            {
                Id = 1,
                Name = "Nguyen Van A"
            };

            // Calculate totals
            decimal subtotal = items.Sum(i => i.LineTotal);
            decimal taxPercent = 10; // 10% VAT
            decimal taxAmount = Math.Round(subtotal * (taxPercent / 100m), 2);
            decimal discount = 500000; // 500k discount
            decimal total = subtotal + taxAmount - discount;

            // Create and show the invoice print window
            var printWindow = new InvoicePrintWindow(
                items, 
                customer, 
                subtotal, 
                taxPercent, 
                taxAmount, 
                discount, 
                total, 
                12345, // Invoice ID
                DateTime.Now
            );
            
            printWindow.ShowDialog();
        }
    }
}
