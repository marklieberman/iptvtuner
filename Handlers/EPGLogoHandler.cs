using IPTVTuner.Model;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Threading.Tasks;
using uhttpsharp;

namespace IPTVTuner.Handlers
{
    /**
     * Generate a logo image with the channel name.
     */
    class EPGLogoHandler : MyHandler, IHttpRequestHandler
    {
        private readonly Config config;
        private readonly Lineup lineup;
        private readonly Color[] palette = new Color[] {
            Color.FromArgb(0xff, 0x89, 0x77),
            Color.FromArgb(0xee, 0xe0, 0xd9),
            Color.FromArgb(0xd2, 0x7b, 0x2c),
            Color.FromArgb(0xff, 0xd2, 0x63),
            Color.FromArgb(0xc9, 0xe0, 0x34),
            Color.FromArgb(0xa9, 0xbf, 0xa6),
            Color.FromArgb(0x52, 0xcd, 0xa5),
            Color.FromArgb(0x6d, 0x98, 0xa2),
            Color.FromArgb(0xbc, 0xd8, 0xf0),
            Color.FromArgb(0xc0, 0xab, 0xff),
            Color.FromArgb(0xff, 0xbe, 0xff),
            Color.FromArgb(0xff, 0x71, 0xbf),
            Color.FromArgb(0xff, 0x32, 0x47),
            Color.FromArgb(0xff, 0xb8, 0xac),
            Color.FromArgb(0xff, 0xb7, 0x86),
            Color.FromArgb(0xc7, 0x94, 0x56),
            Color.FromArgb(0xb7, 0xac, 0x74),
            Color.FromArgb(0xd1, 0xec, 0xa8),
            Color.FromArgb(0x00, 0xff, 0x00),
            Color.FromArgb(0x9b, 0xd1, 0xc6),
            Color.FromArgb(0x46, 0xd2, 0xff),
            Color.FromArgb(0x84, 0x90, 0xb4),
            Color.FromArgb(0xb3, 0x52, 0xff),
            Color.FromArgb(0xff, 0x00, 0xf4),
            Color.FromArgb(0xff, 0x00, 0x88),
            Color.FromArgb(0xff, 0x49, 0x0c),
            Color.FromArgb(0xff, 0x6a, 0x00),
            Color.FromArgb(0xf9, 0xdf, 0xbd),
            Color.FromArgb(0xff, 0xe1, 0x00),
            Color.FromArgb(0x9f, 0xe8, 0x66),
            Color.FromArgb(0x00, 0xae, 0x01),
            Color.FromArgb(0x00, 0xff, 0xff),
            Color.FromArgb(0x00, 0xaa, 0xff),
            Color.FromArgb(0x76, 0x77, 0xff),
            Color.FromArgb(0xa8, 0x85, 0xab),
            Color.FromArgb(0xff, 0x00, 0xb7),
            Color.FromArgb(0xcd, 0x8b, 0x9f),
            Color.FromArgb(0xff, 0x8c, 0x5d),
            Color.FromArgb(0xac, 0x9b, 0x8c),
            Color.FromArgb(0xff, 0xa2, 0x00),
            Color.FromArgb(0xa4, 0xa2, 0x3f),
            Color.FromArgb(0x6c, 0x9d, 0x63),
            Color.FromArgb(0x00, 0xd0, 0x70),
            Color.FromArgb(0x00, 0xab, 0xbb),
            Color.FromArgb(0x00, 0x86, 0xff),
            Color.FromArgb(0xe2, 0xdd, 0xff),
            Color.FromArgb(0xfa, 0x79, 0xff),
            Color.FromArgb(0xcb, 0xb2, 0xc2),
            Color.FromArgb(0xff, 0x6e, 0x93)
        };

        public EPGLogoHandler(Config config, Lineup lineup)
        {
            this.config = config;
            this.lineup = lineup;
        }

        public Task Handle(IHttpContext context, Func<Task> next)
        {
            // Extract the channel number from the request.
            var path = context.Request.Uri.ToString();
            var channelNumber = path.Substring(path.LastIndexOf("/") + 1);

            // Find the actual URL of the stream for the channel.
            var tuning = lineup.Channels.Find(channel => channel.ChannelNumber.Equals(channelNumber));
            if (tuning != null)
            {
                MemoryStream stream = new MemoryStream();
                using (Image icon = GenerateIcon(tuning.Name)) {
                    icon.Save(stream, ImageFormat.Png);
                    stream.Position = 0;
                    context.Response = StreamResponse(context, "image/png", stream);
                }
            }
            else
            {
                // Channel not found.
                context.Response = NotFound();
            }
            
            return Task.Factory.GetCompleted();
        }

        /**
         * Generate a logo image.
         */
        private Image GenerateIcon (string channelName)
        {
            var rect = new RectangleF(0, 0, 128, 128);
            var icon = new Bitmap((int)rect.Width, (int)rect.Height);

            using (Graphics g = Graphics.FromImage(icon))
            using (FontFamily fontFamily = new FontFamily(config.LogoFontFamily))
            using (Font font = new Font(fontFamily, 32, FontStyle.Bold))
            using (Brush colorBrush = new SolidBrush(Color.FromArgb(unchecked((int)config.LogoColor))))            
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;

                // Fill the logo background.
                FillBackground(g, channelName, rect);

                var stringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                // Find the font size to fit the channel name.
                using (Font adjFont = GetAdjustedFont(g, channelName, font, rect.Size, stringFormat, 32, 16))
                {
                    // Draw the channel name,
                    g.DrawString(channelName, adjFont, colorBrush, rect, stringFormat);
                }
            }

            return icon;
        }

        /**
         * Fill the logo background.
         */
        private void FillBackground(Graphics g, string channelName, RectangleF bounds)
        {
            SolidBrush backgroundBrush;
            var argb = unchecked((int)config.LogoBackground);
            if (argb == 0x1)
            {
                // Use a dynamic color.
                var index = Math.Abs(channelName.GetHashCode() % palette.Length);
                var color = Color.FromArgb(128, palette[index]);
                backgroundBrush = new SolidBrush(color);
            }
            else
            {
                // Use the provided color.
                backgroundBrush = new SolidBrush(Color.FromArgb(argb));
            }

            using (backgroundBrush)
            {
                g.FillRectangle(Brushes.Black, bounds);
                g.FillRectangle(backgroundBrush, bounds);
            }
        }

        /**
         * Try to find a font size that fits the channel name in the logo bounds.
         */
        public Font GetAdjustedFont(Graphics g, string graphicString, Font initialFont, SizeF container, StringFormat stringFormat, int maxFontSize, int minFontSize)
        {
            Font adjustedFont = null;
            for (int adjustedSize = maxFontSize; adjustedSize >= minFontSize; adjustedSize--)
            {
                if (adjustedFont != null)
                {
                    adjustedFont.Dispose();
                }

                // MeasureFont appears to return an incorrect height, so use linesFilled as proxy for height.
                adjustedFont = new Font(initialFont.Name, adjustedSize, initialFont.Style);
                SizeF renderedSize = g.MeasureString(graphicString, adjustedFont, container, stringFormat, out _, out int linesFilled);
                if (renderedSize.Width <= container.Width && linesFilled <= 2)
                {
                    return adjustedFont;
                }
            }

            return adjustedFont;
        }
    }
}
