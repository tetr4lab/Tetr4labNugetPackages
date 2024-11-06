using System;
using System.IO;
using System.Reflection;

namespace Tetr4lab {

    /// <summary>リビジョン情報</summary>
    /// <example><code>
    /// if (RevisionInfo.Valid) Console.WriteLine ($"revision: {RevisionInfo.Branch} {RevisionInfo.Id}");
    /// </code></example>
    /// <settings>
    /// - プロジェクトのプロパティ > ビルド > イベント > ビルド前のイベント
    ///   git branch --show-current > $(ProjectDir)revision.info
    ///   git rev-parse --short HEAD >> $(ProjectDir) revision.info
    /// - `$(ProjectDir)revision.info`のプロパティ
    ///   ビルドアクションを「埋め込みリソース」に設定
    /// </settings>
    /// <see href="https://qiita.com/hqf00342/items/b5afa3e6ebc3551884a4">[.NET/C#] アプリにgitのコミットIDを埋め込む</see>
    public static partial class RevisionInfo {

        /// <summary>有効性</summary>
        public static bool Valid => Branch is not null && Id is not null;

        /// <summary>ブランチ</summary>
        public static string Branch {
            get {
                if (_branch is null) {
                    Initialize (Assembly.GetEntryAssembly ());
                }
                return _branch!;
            }
        }
        private static string? _branch;

        /// <summary>コミットID</summary>
        public static string Id {
            get {
                if (_id is null) {
                    Initialize (Assembly.GetEntryAssembly ());
                }
                return _id!;
            }
        }
        private static string? _id;

        /// <summary>リソースファイル名</summary>
        private const string ResourceName = "revision.info";

        /// <summary>初期化</summary>
        /// <param name="asm">アセンブリ</param>
        private static void Initialize (Assembly? asm) {
            if (asm is null) { return; }
            var resName = Array.Find (asm.GetManifestResourceNames (), n => n.EndsWith (ResourceName));
            if (resName != null) {
                using (var stream = asm.GetManifestResourceStream (resName)) {
                    if (stream != null) {
                        var lines = (new StreamReader (stream)).ReadToEnd ().Replace ("\r\n", "\n").Split (['\r', '\n']);
                        if (lines.Length > 1) {
                            _branch = lines [0].Trim ();
                            _id = lines [1].Trim ();
                        }
                    }
                }
            }
        }

    }

}
