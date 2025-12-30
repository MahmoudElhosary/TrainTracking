using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TrainTracking.Domain.Entities;
using QRCoder;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using SkiaSharp;

namespace TrainTracking.Web.Services
{
    public class TicketGenerator
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TicketGenerator(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public byte[] GenerateTicketPdf(Booking booking, string qrUrl)
        {
            // Generate QR Code
            byte[] qrCodeImage = GenerateQrCode(qrUrl);
            string logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "logo.png");
            byte[]? logoImage = File.Exists(logoPath) ? File.ReadAllBytes(logoPath) : null;

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5.Landscape());
                    page.Margin(0);
                    page.PageColor(Colors.White);
                    // Use Segoe UI for better Arabic support on Windows, fallback to Arial
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Segoe UI"));

                    page.Content().Element(content =>
                    {
                        content.Column(col => 
                        {
                            // Header Section
                            col.Item().Height(80).Background("#1e3c72").PaddingHorizontal(20).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text(text => 
                                    {
                                        text.Span("KuwGo").FontSize(20).Bold().FontColor(Colors.White);
                                    });
                                    c.Item().Text("OFFICIAL E-TICKET").FontSize(10).LetterSpacing(2).FontColor(Colors.Grey.Lighten3);
                                });

                                if (logoImage != null)
                                {
                                    row.ConstantItem(100).AlignRight().AlignMiddle().Image(logoImage).FitArea();
                                }
                                else
                                {
                                    row.ConstantItem(60).AlignRight().AlignMiddle().Text("KUW").FontSize(25).FontColor(Colors.White).Bold();
                                }
                            });

                            // Main Body
                            col.Item().Padding(25).Row(mainRow =>
                            {
                                // Left Side: QR & ID
                                mainRow.ConstantItem(120).Column(c =>
                                {
                                    c.Item().AlignCenter().Image(qrCodeImage).FitArea();
                                    c.Item().PaddingTop(10).AlignCenter().Text($"ID: {booking.Id.ToString().Substring(0, 8).ToUpper()}").FontSize(8).Bold();
                                    c.Item().AlignCenter().Text("Scan to verify").FontSize(6).FontColor(Colors.Grey.Medium);
                                    c.Item().AlignCenter().Text(qrUrl).FontSize(6).FontColor(Colors.Grey.Darken1);
                                });
                                
                                mainRow.ConstantItem(20); 

                                // Right Side: Details
                                mainRow.RelativeItem().Column(details =>
                                {
                                    if (booking.Trip != null)
                                    {
                                        // Row 1: Passenger
                                        details.Item().Row(r =>
                                        {
                                            r.RelativeItem().Column(c =>
                                            {
                                                c.Item().Text("المسافر / Passenger").FontSize(8).FontColor(Colors.Grey.Medium);
                                                c.Item().Text(booking.PassengerName ?? "Unknown").FontSize(14).Bold().FontColor("#2c3e50");
                                            });

                                            r.RelativeItem().Column(c =>
                                            {
                                                c.Item().Text("التاريخ / Date").FontSize(8).FontColor(Colors.Grey.Medium);
                                                c.Item().Text(booking.Trip.DepartureTime.ToString("yyyy-MM-dd")).FontSize(14).Bold().FontColor("#2c3e50");
                                            });
                                        });

                                        details.Item().PaddingVertical(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten3);

                                        // Row 2: Journey
                                        details.Item().Row(r =>
                                        {
                                            r.RelativeItem().Column(c =>
                                            {
                                                c.Item().Text("من / From").FontSize(8).FontColor(Colors.Grey.Medium);
                                                c.Item().Text(booking.Trip.FromStation?.Name ?? "Unknown").FontSize(16).Bold().FontColor("#e74c3c");
                                                c.Item().Text(booking.Trip.DepartureTime.ToString("HH:mm")).FontSize(12);
                                            });
                                            
                                            // Replaced Emoji with Text Arrow
                                            r.ConstantItem(40).AlignCenter().PaddingTop(10).Text("-->").FontSize(20).FontColor(Colors.Grey.Medium);

                                            r.RelativeItem().Column(c =>
                                            {
                                                c.Item().Text("إلى / To").FontSize(8).FontColor(Colors.Grey.Medium);
                                                c.Item().Text(booking.Trip.ToStation?.Name ?? "Unknown").FontSize(16).Bold().FontColor("#e74c3c");
                                                c.Item().Text(booking.Trip.ArrivalTime.ToString("HH:mm")).FontSize(12);
                                            });
                                        });

                                        details.Item().PaddingVertical(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten3);

                                        // Row 3: Train Info
                                        details.Item().Row(r =>
                                        {
                                            r.RelativeItem().Column(c =>
                                            {
                                                c.Item().Text("القطار / Train").FontSize(8).FontColor(Colors.Grey.Medium);
                                                c.Item().Text(booking.Trip.Train?.TrainNumber ?? "Unknown").FontSize(12).Bold();
                                            });

                                            r.RelativeItem().Column(c =>
                                            {
                                                c.Item().Text("المقعد / Seat").FontSize(8).FontColor(Colors.Grey.Medium);
                                                c.Item().Text(booking.SeatNumber.ToString()).FontSize(12).Bold();
                                            });

                                            r.RelativeItem().Column(c =>
                                            {
                                                c.Item().Text("السعر / Price").FontSize(8).FontColor(Colors.Grey.Medium);
                                                c.Item().Text($"{booking.Price:F2} KD").FontSize(12).Bold();
                                            });
                                        });
                                    }
                                });
                            });

                            // Footer
                            col.Item().Background(Colors.Grey.Lighten5).Padding(10).AlignCenter().Row(row => 
                            {
                                row.RelativeItem().AlignCenter().Text("نتمنى لكم رحلة سعيدة - Have a safe trip").FontSize(8).Italic().FontColor(Colors.Grey.Darken1);
                            });
                            col.Item().Height(5).Background("#2ecc71");
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        private byte[] GenerateQrCode(string url)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                {
                    return qrCode.GetGraphic(20);
                }
            }
        }
    }
}
