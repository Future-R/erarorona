using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using MinorShift._Library;
using MinorShift.Emuera.GameView;
using MinorShift.Emuera.GameData.Expression;
using System.IO;

namespace MinorShift.Emuera
{
	static class Program
	{
		/*
		コードの開始地点。
		ここでMainWindowを作り、
		MainWindowがProcessを作り、
		ProcessがGameBase・ConstantData・Variableを作る。
		
		
		*.ERBの読み込み、実行、その他の処理をProcessが、
		入出力をMainWindowが、
		定数の保存をConstantDataが、
		変数の管理をVariableが行う。
		 
		と言う予定だったが改変するうちに境界が曖昧になってしまった。
		 
		後にEmueraConsoleを追加し、それに入出力を担当させることに。
        
        1750 DebugConsole追加
         * Debugを全て切り離すことはできないので一部EmueraConsoleにも担当させる
		*/
		/// <summary>
		/// アプリケーションのメイン エントリ ポイントです。
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{

#if DEBUG
			ExeDir = Sys.ExeDir;
#else
			ExeDir = Sys.ExeDir;
#endif
			CsvDir = ExeDir + "csv\\";
			ErbDir = ExeDir + "erb\\";
			DebugDir = ExeDir + "debug\\";
			DatDir = ExeDir + "dat\\";
			ExeName = Sys.ExeName;

            int oldConfigx = 0;
            int oldConfigy = 0;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			ConfigData.Instance.LoadConfig();
            //二重起動の禁止かつ二重起動
			if ((!Config.AllowMultipleInstances) && (Sys.PrevInstance()))
			{
				MessageBox.Show("多重起動を許可する場合、emuera.configを書き換えて下さい", "既に起動しています");
				return;
			}
			if (!Directory.Exists(CsvDir))
			{
				MessageBox.Show("csvフォルダが見つかりません", "フォルダなし");
				return;
			}
			if (!Directory.Exists(ErbDir))
			{
				MessageBox.Show("erbフォルダが見つかりません", "フォルダなし");
				return;
			}
            int argsStart = 0;
            if ((args.Length > 0)&&(args[0].Equals("-DEBUG", StringComparison.CurrentCultureIgnoreCase)))
            {
				ConfigData.Instance.LoadDebugConfig();
                argsStart = 1;//デバッグモードかつ解析モード時に最初の1っこ(-DEBUG)を飛ばす
				if (!Directory.Exists(DebugDir))
				{
					try
					{
						Directory.CreateDirectory(DebugDir);
					}
					catch
					{
						MessageBox.Show("debugフォルダの作成に失敗しました", "フォルダなし");
						return;
					}
				}
				debugMode = true;
            }
            if (args.Length > argsStart)
            {
                AnalysisFiles = new List<string>();
                for (int i = argsStart; i < args.Length; i++)
                {
                    if (!File.Exists(args[i]) && !Directory.Exists(args[i]))
                    {
                        MessageBox.Show("与えられたファイル・フォルダは存在しません");
                        return;
                    }
                    if ((File.GetAttributes(args[i]) & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        List<KeyValuePair<string, string>> fnames = Config.GetFiles(args[i] + "\\", "*.ERB");
                        for (int j = 0; j < fnames.Count; j++)
                        {
                            AnalysisFiles.Add(fnames[j].Value);
                        }
                    }
                    else
                    {
                        if (Path.GetExtension(args[i]).ToUpper() != ".ERB")
                        {
                            MessageBox.Show("ドロップ可能なファイルはERBファイルのみです");
                            return;
                        }
                        AnalysisFiles.Add(args[i]);
                    }
                }
                AnalysisMode = true;
            }
			while (true)
			{
				StartTime = WinmmTimer.TickCount;
				Application.Run(new MainWindow());
                if (!Reboot)
					break;
				//条件次第ではParserMediatorが空でない状態で再起動になる場合がある
                ParserMediator.ClearWarningList();
				ParserMediator.Initialize(null);
				GlobalStatic.Reset();
				GC.Collect();
				Reboot = false;
                oldConfigx = Config.WindowX;
                oldConfigy = Config.WindowY;
				ConfigData.Instance.LoadConfig();
                int winX = ClientX;
                if (oldConfigx != Config.WindowX)
                {
                    ClientX = 0;
                    winX = Config.WindowX;
                }
                if (oldConfigy != Config.WindowY)
                    ClientY = 0;
                StringMeasure.setStaticMember(winX);
            }
		}

		/// <summary>
		/// 実行ファイルのディレクトリ。最後に\を付けたstring
		/// </summary>
		public static string ExeDir { get; private set; }
		public static string CsvDir { get; private set; }
		public static string ErbDir { get; private set; }
		public static string DebugDir { get; private set; }
		public static string DatDir { get; private set; }
		public static string ExeName { get; private set; }

		public static bool Reboot = false;
        public static int ClientX = 0;
        public static int ClientY = 0;
        public static FormWindowState state = FormWindowState.Normal;
        public static bool AnalysisMode = false;
        public static List<string> AnalysisFiles = null;

		public static bool debugMode = false;
		public static bool DebugMode { get { return debugMode; } }


		public static uint StartTime { get; private set; }

	}
}