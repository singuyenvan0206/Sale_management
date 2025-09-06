using System;
using System.Collections.Generic;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Lớp demo để kiểm tra chức năng in hóa đơn
    /// </summary>
    public static class InvoiceDemo
    {
        public static void ShowDemoInvoice()
        {
            // Tạo dữ liệu mẫu hóa đơn
            var items = new List<InvoiceItemViewModel>
            {
                new InvoiceItemViewModel
                {
                    ProductId = 1,
                    ProductName = "Laptop Dell Inspiron 15",
                    UnitPrice = 15000000,
                    Quantity = 1,
                    LineTotal = 15000000
                },
                new InvoiceItemViewModel
                {
                    ProductId = 2,
                    ProductName = "Chuột không dây Logitech",
                    UnitPrice = 500000,
                    Quantity = 2,
                    LineTotal = 1000000
                },
                new InvoiceItemViewModel
                {
                    ProductId = 3,
                    ProductName = "Hub USB-C đa năng",
                    UnitPrice = 800000,
                    Quantity = 1,
                    LineTotal = 800000
                },
                new InvoiceItemViewModel
                {
                    ProductId = 4,
                    ProductName = "Bàn phím cơ gaming",
                    UnitPrice = 1200000,
                    Quantity = 1,
                    LineTotal = 1200000
                },
                new InvoiceItemViewModel
                {
                    ProductId = 5,
                    ProductName = "Tai nghe Bluetooth Sony",
                    UnitPrice = 2500000,
                    Quantity = 1,
                    LineTotal = 2500000
                }
            };

            // Tạo khách hàng mẫu
            var customer = new CustomerListItem
            {
                Id = 1,
                Name = "Nguyễn Văn An"
            };

            // Tính toán tổng tiền
            decimal subtotal = items.Sum(i => i.LineTotal);
            decimal taxPercent = 10; // Thuế VAT 10%
            decimal taxAmount = Math.Round(subtotal * (taxPercent / 100m), 2);
            decimal discount = 1000000; // Giảm giá 1 triệu đồng
            decimal total = subtotal + taxAmount - discount;

            // Tạo và hiển thị cửa sổ in hóa đơn
            var printWindow = new InvoicePrintWindow(
                items, 
                customer, 
                subtotal, 
                taxPercent, 
                taxAmount, 
                discount, 
                total, 
                2024001, // Mã hóa đơn
                DateTime.Now
            );
            
            printWindow.ShowDialog();
        }

        /// <summary>
        /// Hiển thị hóa đơn demo cho cửa hàng thực phẩm
        /// </summary>
        public static void ShowFoodStoreDemo()
        {
            // Tạo dữ liệu mẫu cho cửa hàng thực phẩm
            var items = new List<InvoiceItemViewModel>
            {
                new InvoiceItemViewModel
                {
                    ProductId = 1,
                    ProductName = "Cơm tấm sườn nướng",
                    UnitPrice = 35000,
                    Quantity = 2,
                    LineTotal = 70000
                },
                new InvoiceItemViewModel
                {
                    ProductId = 2,
                    ProductName = "Phở bò tái",
                    UnitPrice = 45000,
                    Quantity = 1,
                    LineTotal = 45000
                },
                new InvoiceItemViewModel
                {
                    ProductId = 3,
                    ProductName = "Bánh mì thịt nướng",
                    UnitPrice = 15000,
                    Quantity = 3,
                    LineTotal = 45000
                },
                new InvoiceItemViewModel
                {
                    ProductId = 4,
                    ProductName = "Coca Cola",
                    UnitPrice = 12000,
                    Quantity = 2,
                    LineTotal = 24000
                },
                new InvoiceItemViewModel
                {
                    ProductId = 5,
                    ProductName = "Trà sữa trân châu",
                    UnitPrice = 25000,
                    Quantity = 1,
                    LineTotal = 25000
                }
            };

            // Tạo khách hàng mẫu
            var customer = new CustomerListItem
            {
                Id = 2,
                Name = "Trần Thị Bình"
            };

            // Tính toán tổng tiền
            decimal subtotal = items.Sum(i => i.LineTotal);
            decimal taxPercent = 10; // Thuế VAT 10%
            decimal taxAmount = Math.Round(subtotal * (taxPercent / 100m), 2);
            decimal discount = 0; // Không giảm giá
            decimal total = subtotal + taxAmount - discount;

            // Tạo và hiển thị cửa sổ in hóa đơn
            var printWindow = new InvoicePrintWindow(
                items, 
                customer, 
                subtotal, 
                taxPercent, 
                taxAmount, 
                discount, 
                total, 
                2024002, // Mã hóa đơn
                DateTime.Now
            );
            
            printWindow.ShowDialog();
        }

        /// <summary>
        /// Hiển thị hóa đơn demo cho cửa hàng điện tử
        /// </summary>
        public static void ShowElectronicsDemo()
        {
            // Tạo dữ liệu mẫu cho cửa hàng điện tử
            var items = new List<InvoiceItemViewModel>
            {
                new InvoiceItemViewModel
                {
                    ProductId = 1,
                    ProductName = "iPhone 15 Pro Max 256GB",
                    UnitPrice = 35000000,
                    Quantity = 1,
                    LineTotal = 35000000
                },
                new InvoiceItemViewModel
                {
                    ProductId = 2,
                    ProductName = "AirPods Pro 2",
                    UnitPrice = 5500000,
                    Quantity = 1,
                    LineTotal = 5500000
                },
                new InvoiceItemViewModel
                {
                    ProductId = 3,
                    ProductName = "Ốp lưng iPhone 15 Pro Max",
                    UnitPrice = 500000,
                    Quantity = 2,
                    LineTotal = 1000000
                },
                new InvoiceItemViewModel
                {
                    ProductId = 4,
                    ProductName = "Cáp sạc USB-C 2m",
                    UnitPrice = 300000,
                    Quantity = 1,
                    LineTotal = 300000
                }
            };

            // Tạo khách hàng mẫu
            var customer = new CustomerListItem
            {
                Id = 3,
                Name = "Lê Văn Cường"
            };

            // Tính toán tổng tiền
            decimal subtotal = items.Sum(i => i.LineTotal);
            decimal taxPercent = 10; // Thuế VAT 10%
            decimal taxAmount = Math.Round(subtotal * (taxPercent / 100m), 2);
            decimal discount = 2000000; // Giảm giá 2 triệu đồng
            decimal total = subtotal + taxAmount - discount;

            // Tạo và hiển thị cửa sổ in hóa đơn
            var printWindow = new InvoicePrintWindow(
                items, 
                customer, 
                subtotal, 
                taxPercent, 
                taxAmount, 
                discount, 
                total, 
                2024003, // Mã hóa đơn
                DateTime.Now
            );
            
            printWindow.ShowDialog();
        }
    }
}
