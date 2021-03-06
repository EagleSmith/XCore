﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XCore.Modules;

namespace XCore.Mvc.Core
{
    ///// <summary>
    ///// This custom <see cref="IFileProvider"/> implementation provides the file contents
    ///// of Module Project Razor files while in a development environment.
    ///// </summary>
    //public class ModuleProjectRazorFileProvider : IFileProvider
    //{
    //    private const string MappingFileFolder = "obj";
    //    private const string MappingFileName = "ModuleProjectRazorFiles.map";

    //    private static Dictionary<string, string> _paths;
    //    private static CompositeFileProvider _pagesFileProvider;
    //    private static object _synLock = new object();

    //    public ModuleProjectRazorFileProvider(string rootPath)
    //    {
    //        if (_paths != null)
    //        {
    //            return;
    //        }

    //        lock (_synLock)
    //        {
    //            if (_paths == null)
    //            {
    //                var path = Path.Combine(rootPath, MappingFileFolder, MappingFileName);

    //                if (File.Exists(path))
    //                {
    //                    var paths = File.ReadAllLines(path)
    //                        .Select(x => x.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
    //                        .Where(x => x.Length == 2).ToDictionary(x => x[1].Replace('\\', '/'), x => x[0].Replace('\\', '/'));

    //                    _paths = new Dictionary<string, string>(paths);
    //                }
    //                else
    //                {
    //                    _paths = new Dictionary<string, string>();
    //                }
    //            }

    //            var roots = new HashSet<string>();

    //            foreach (var path in _paths.Values.Where(p => p.Contains("/Pages/") && !p.StartsWith("/Pages/")))
    //            {
    //                roots.Add(path.Substring(0, path.IndexOf("/Pages/")));
    //            }

    //            if (roots.Count > 0)
    //            {
    //                _pagesFileProvider = new CompositeFileProvider(roots.Select(r => new PhysicalFileProvider(r)));
    //            }
    //        }
    //    }

    //    public IDirectoryContents GetDirectoryContents(string subpath)
    //    {
    //        return null;
    //    }

    //    public IFileInfo GetFileInfo(string subpath)
    //    {
    //        if (subpath != null && _paths.ContainsKey(subpath))
    //        {
    //            return new PhysicalFileInfo(new FileInfo(_paths[subpath]));
    //        }

    //        return null;
    //    }

    //    public IChangeToken Watch(string filter)
    //    {
    //        if (filter != null && _paths.ContainsKey(filter))
    //        {
    //            return new PollingFileChangeToken(new FileInfo(_paths[filter]));
    //        }

    //        if (filter != null && _pagesFileProvider != null &&
    //            filter.IndexOf("/Pages/**/*" + RazorViewEngine.ViewExtension) != -1)
    //        {
    //            return _pagesFileProvider.Watch("/Pages/**/*" + RazorViewEngine.ViewExtension);
    //        }

    //        return null;
    //    }
    //}

    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the file contents
    /// of Module Project Razor files while in a development environment.
    /// </summary>
    public class ModuleProjectRazorFileProvider : IFileProvider
    {
        private static Dictionary<string, string> _paths;
        private static object _synLock = new object();

        public ModuleProjectRazorFileProvider(IHostingEnvironment environment)
        {
            if (_paths != null)
            {
                return;
            }

            lock (_synLock)
            {
                if (_paths == null)
                {
                    var assets = new List<Asset>();
                    var application = environment.GetApplication();

                    foreach (var name in application.ModuleNames)
                    {
                        var module = environment.GetModule(name);

                        if (module.Assembly == null || Path.GetDirectoryName(module.Assembly.Location)
                            != Path.GetDirectoryName(application.Assembly.Location))
                        {
                            continue;
                        }

                        assets.AddRange(module.Assets.Where(a => a.ModuleAssetPath
                            .EndsWith(".cshtml", StringComparison.Ordinal)));
                    }

                    _paths = assets.ToDictionary(a => a.ModuleAssetPath, a => a.ProjectAssetPath);
                }
            }
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return NotFoundDirectoryContents.Singleton;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (subpath == null)
            {
                return new NotFoundFileInfo(subpath);
            }

            var path = NormalizePath(subpath);

            if (_paths.ContainsKey(path))
            {
                return new PhysicalFileInfo(new FileInfo(_paths[path]));
            }

            return new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            if (filter == null)
            {
                return NullChangeToken.Singleton;
            }

            var path = NormalizePath(filter);

            if (_paths.ContainsKey(path))
            {
                return new PollingFileChangeToken(new FileInfo(_paths[path]));
            }

            return NullChangeToken.Singleton;
        }

        private string NormalizePath(string path)
        {
            return path.Replace('\\', '/').Trim('/');
        }
    }
}
