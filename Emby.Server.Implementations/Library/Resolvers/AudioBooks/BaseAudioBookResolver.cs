#nullable disable

#pragma warning disable CS1591

using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Emby.Naming.Common;
using MediaBrowser.Controller.Entities.AudioBooks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using Microsoft.Extensions.Logging;

namespace Emby.Server.Implementations.Library.Resolvers.AudioBooks
{
    /// <summary>
    /// Resolves a Path into an AudioBookFile.
    /// </summary>
    /// <typeparam name="T">The type of item to resolve.</typeparam>
    public abstract class BaseAudioBookResolver<T> : MediaBrowser.Controller.Resolvers.ItemResolver<T>
        where T : AudioBookFile, new()
    {
        private readonly ILogger _logger;

        protected BaseAudioBookResolver(ILogger logger, NamingOptions namingOptions, IDirectoryService directoryService)
        {
            _logger = logger;
            NamingOptions = namingOptions;
            DirectoryService = directoryService;
        }

        protected NamingOptions NamingOptions { get; }

        protected IDirectoryService DirectoryService { get; }

        /// <summary>
        /// Abstract "Resolve" function used to redirect to specific, internal resolve function.
        /// </summary>
        /// <param name="args">Object containing what's currently known about the resolution target (usually just path).</param>
        /// <returns>Populated instance of type T (AudioBookFile) resolved from args.Path.</returns>
        protected override T Resolve(ItemResolveArgs args)
        {
            return ResolveAudioBookFile<T>(args, false);
        }

        /// <summary>
        /// Resolves the video.
        /// </summary>
        /// <typeparam name="TAudioBookType">Abstract return type - only supports AudioBookFile.</typeparam>
        /// <param name="args">Object containing what's currently known about the resolution target (usually just path).</param>
        /// <param name="parseName">Ignored.</param>
        /// <returns>``0.</returns>
        protected virtual TAudioBookType ResolveAudioBookFile<TAudioBookType>(ItemResolveArgs args, bool parseName)
              where TAudioBookType : AudioBookFile, new()
        {
            var audioBookFile = new TAudioBookType
            {
                Path = args.Path
            };

            // Get AudioBookFile info
            var extension = Path.GetExtension(args.Path);

            audioBookFile.Container = extension.TrimStart('.');

            audioBookFile.IndexNumber = ParseChapter(args.Path);

            return audioBookFile;
        }

        private int ParseChapter(string path)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            foreach (var expression in NamingOptions.AudioBookPartsExpressions)
            {
                var match = Regex.Match(fileName, expression, RegexOptions.IgnoreCase);
                while (match.Success)
                {
                    if (int.TryParse(match.ValueSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
                    {
                        return intValue;
                    }

                    match = match.NextMatch();
                }
            }

            return 0;
        }
    }
}