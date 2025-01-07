using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using MinorShift._Library;
using MinorShift.Emuera.Sub;
using MinorShift.Emuera.GameData;
using MinorShift.Emuera.GameProc;
using System.Drawing.Imaging;
using MinorShift.Emuera.Forms;
using MinorShift.Emuera.GameData.Expression;
using MinorShift.Emuera.GameProc.Function;

namespace MinorShift.Emuera.GameView
{
	//入出力待ちの状況。
	//難読化用属性。enum.ToString()やenum.Parse()を行うなら(Exclude=true)にすること。
	[global::System.Reflection.Obfuscation(Exclude=false)]
	internal enum ConsoleState
	{
		Initializing = 0,
		WaitKey = 1,//WAIT
		WaitSystemInteger = 2,//Systemが要求するInput
		WaitInteger = 3,//INPUT
		WaitString = 4,//INPUTS
		Quit = 5,//QUIT
		Error = 6,//Exceptionによる強制終了
		Running = 7,
		WaitIntegerWithTimer = 8,
		WaitStringWithTimer = 9,
		Timeout = 10,
		Timeouts = 11,
		WaitKeyWithTimer = 12,
		WaitKeyWithTimerF = 13,
        WaitOneInteger = 14,
        WaitOneString = 15,
        WaitOneIntegerWithTimer = 16,
        WaitOneStringWithTimer = 17,
        WaitAnyKey = 18,
    }

	//難読化用属性。enum.ToString()やenum.Parse()を行うなら(Exclude=true)にすること。
	[global::System.Reflection.Obfuscation(Exclude=false)]
	internal enum ConsoleRedraw
	{
		None = 0,
		Normal = 1,
	}

	internal sealed class EmueraConsole
	{
		public EmueraConsole(MainWindow parent)
		{
			window = parent;

			//1.713 この段階でsetStBarを使用してはいけない
			//setStBar(StaticConfig.DrawLineString);
			state = ConsoleState.Initializing;
			if (Config.FPS > 0)
				msPerFrame = 1000 / (uint)Config.FPS;
			displayLineList = new List<ConsoleDisplayLine>();
			printBuffer = new PrintStringBuffer(this);
			MainPicBoxSizeChanged();
        }

        private readonly MainWindow window;

		PrintStringBuffer printBuffer;
		MinorShift.Emuera.GameProc.Process emuera;
		ConsoleState state = ConsoleState.Initializing;
		internal ConsoleState State { get { return state; } }
        public bool noOutputLog = false;
        public Color bgColor = Config.BackColor;

		public bool Enabled { get { return window.Created; } }
        internal bool IsInitializing
        {
            get { return (state == ConsoleState.Initializing); }
        }
		internal bool IsRunning
		{
			get
			{
				if (state == ConsoleState.Initializing)
					return true;
				return (state == ConsoleState.Running || isDebug);
			}
		}
		internal bool IsError
		{
			get
			{
				return state == ConsoleState.Error;
			}
		}

		internal bool IsWaitingAnyKey
		{
			get
			{
				return ((state == ConsoleState.WaitKey) || (state == ConsoleState.Quit) || (state == ConsoleState.Error) || (state == ConsoleState.WaitKeyWithTimer) || (state == ConsoleState.WaitAnyKey));
			}
		}

        internal bool IsWaitAnyKeyAndGo
        {
            get
            {
                return (state == ConsoleState.WaitAnyKey);
            }
        }

        internal bool IsWaintingOnePhrase
        {
            get
            {
                return (state == ConsoleState.WaitOneInteger || state == ConsoleState.WaitOneString || state == ConsoleState.WaitOneIntegerWithTimer || state == ConsoleState.WaitOneStringWithTimer);
            }
        }
        internal bool IsRunningTimer
        {
            get
            {
                return ((state == ConsoleState.WaitIntegerWithTimer) || (state == ConsoleState.WaitStringWithTimer) || (state == ConsoleState.WaitOneIntegerWithTimer) || (state == ConsoleState.WaitOneStringWithTimer) || (state == ConsoleState.WaitKeyWithTimer) || (state == ConsoleState.WaitKeyWithTimerF));
            }
        }
		internal string SelectedString
		{
			get
			{
				if (selectingButton == null)
					return null;
				if ((state == ConsoleState.WaitInteger || state == ConsoleState.WaitIntegerWithTimer || state == ConsoleState.WaitOneInteger || state == ConsoleState.WaitOneIntegerWithTimer) && (selectingButton.IsInteger))
					return selectingButton.Input.ToString();
				if ((state == ConsoleState.WaitSystemInteger) && (selectingButton.IsInteger))
					return selectingButton.Input.ToString();
				if (state == ConsoleState.WaitString || state == ConsoleState.WaitStringWithTimer || state == ConsoleState.WaitOneString || state == ConsoleState.WaitOneStringWithTimer || state == ConsoleState.Error)
					return selectingButton.Inputs;
				return null;
			}
		}

		internal ScriptPosition SelectedPos
		{
			get
			{
				if (selectingButton == null)
					return null;
				return selectingButton.ErrPos;
			}
		}

		public void Initialize()
		{
			GlobalStatic.Console = this;
			GlobalStatic.MainWindow = window;
            emuera = new GameProc.Process(this);
			GlobalStatic.Process = emuera;
			if (Program.DebugMode && Config.DebugShowWindow)
			{
				OpenDebugDialog();
				window.Focus();
			}
			ClearDisplay();
			if (!emuera.Initialize())
			{
				state = ConsoleState.Error;
				outputLog(null);
				PrintFlush(false);
				RefreshStrings(true);
				return;
			}
			callEmueraProgram("");
			RefreshStrings(true);
		}

		public void ClearDisplay()
		{
			Graphics graph = getGraphics();
            graph.Clear(this.bgColor);
			graph.Dispose();
		}
        bool hasDefValue = false;
        Int64 defNum;
        string defStr;
		public void ReadInteger() { state = ConsoleState.WaitInteger; }
        public void ReadInteger(Int64 def) { state = ConsoleState.WaitInteger; defNum = def; hasDefValue = true; }
        public void ReadOneInteger() { state = ConsoleState.WaitOneInteger; window.update_lastinput(); }
        public void ReadOneInteger(Int64 def) { state = ConsoleState.WaitOneInteger; defNum = def; hasDefValue = true; window.update_lastinput(); }
        public void ReadSystemInteger() { state = ConsoleState.WaitSystemInteger; }
		public void ReadString() { state = ConsoleState.WaitString; }
        public void ReadString(string def) { state = ConsoleState.WaitString; defStr = def; hasDefValue = true; }
        public void ReadOneString() { state = ConsoleState.WaitOneString; window.update_lastinput(); }
        public void ReadOneString(string def) { state = ConsoleState.WaitOneString; defStr = def; hasDefValue = true; window.update_lastinput(); }
        public void ReadAnyKey()
		{
			emuera.NeedWaitToEventComEnd = false;
			state = ConsoleState.WaitKey;
		}
        public void ReadAnyKeyAndGo()
        {
            emuera.NeedWaitToEventComEnd = false;
            state = ConsoleState.WaitAnyKey;
        }
        public void ReadAnyKeyForce()
        {
            emuera.NeedWaitToEventComEnd = false;
            state = ConsoleState.WaitKey;
            isForceWait = true;
        }
        public void Quit() { state = ConsoleState.Quit; }
        public void ThrowTitleError(bool error) { state = ConsoleState.Error; notToTitle = true; byError = error; }
		public void ThrowError(bool playSound)
		{
			if (playSound)
				System.Media.SystemSounds.Hand.Play();
			forceUpdateGeneration();
			UseUserStyle = false;
			PrintFlush(false);
			state = ConsoleState.Error;
		}

        public bool notToTitle = false;
        public bool byError = false;
        //public ScriptPosition ErrPos = null;

		#region button関連
		bool lastButtonIsInput = true;
        public bool updatedGeneration = false;
		int lastButtonGeneration = 0;//最後に追加された選択肢の世代。これと世代が一致しない選択肢は選択できない。
		int newButtonGeneration = 0;//次に追加される選択肢の世代。Input又はInputsごとに増加
		//public int LastButtonGeneration { get { return lastButtonGeneration; } }
		public int NewButtonGeneration { get { return newButtonGeneration; } }
        public void UpdateGeneration() { lastButtonGeneration = newButtonGeneration; updatedGeneration = true; }
        public void forceUpdateGeneration() { newButtonGeneration++; lastButtonGeneration = newButtonGeneration; updatedGeneration = true; }
        LogicalLine lastInputLine;

		private void newGeneration()
		{
            //値の入力を求められない時は更新は必要ないはず
            if (state != ConsoleState.WaitInteger && state != ConsoleState.WaitIntegerWithTimer && state != ConsoleState.WaitString && state != ConsoleState.WaitStringWithTimer && state != ConsoleState.WaitSystemInteger
                && state != ConsoleState.WaitOneInteger && state != ConsoleState.WaitOneString && state != ConsoleState.WaitOneIntegerWithTimer && state != ConsoleState.WaitOneStringWithTimer)
                return;
            if (!updatedGeneration && emuera.getCurrentLine != lastInputLine)
            {
                //ボタン無しで次の入力に来たなら強制で世代更新
                lastButtonGeneration = newButtonGeneration;
            }
            else
                updatedGeneration = false;
            lastInputLine = emuera.getCurrentLine;
			//古い選択肢を選択できないように。INPUTで使った選択肢をINPUTSには流用できないように。
			if ((state == ConsoleState.WaitInteger) || (state == ConsoleState.WaitSystemInteger) || (state == ConsoleState.WaitIntegerWithTimer) || (state == ConsoleState.WaitOneInteger) || (state == ConsoleState.WaitOneIntegerWithTimer))
			{
				if (lastButtonGeneration == newButtonGeneration)
					unchecked { newButtonGeneration++; }
				else if (!lastButtonIsInput)
					lastButtonGeneration = newButtonGeneration;
				lastButtonIsInput = true;
			}
			if (state == ConsoleState.WaitString || state == ConsoleState.WaitStringWithTimer || state == ConsoleState.WaitOneString || state == ConsoleState.WaitOneStringWithTimer)
			{
				if (lastButtonGeneration == newButtonGeneration)
					unchecked { newButtonGeneration++; }
				else if (lastButtonIsInput)
					lastButtonGeneration = newButtonGeneration;
				lastButtonIsInput = false;
			}
		}

		/// <summary>
		/// 選択中のボタン。INPUTやINPUTSに対応したものでなければならない
		/// </summary>
		ConsoleButtonString selectingButton = null;
		ConsoleButtonString lastSelectingButton = null;
		public ConsoleButtonString SelectingButton { get { return selectingButton; } }
		public bool ButtonIsSelected(ConsoleButtonString button) { return selectingButton == button; }

		/// <summary>
		/// ToolTip表示したフラグ
		/// </summary>
		bool tooltipUsed = false;
		/// <summary>
		/// マウスの直下にあるテキスト。ボタンであってもよい。
		/// ToolTip表示用。世代無視、履歴中も表示
		/// </summary>
		ConsoleButtonString pointingString = null;
		ConsoleButtonString lastPointingString = null;
		#endregion

		#region Timer系
		Timer timer;
		Int64 result = 0;
        string results = "";
        int countTime = 0;
		int timeLimit = 0;
		bool isDisp;
        bool wait_timeout = false;
        string timeup_label;
        bool isTimeout = false;
        public bool IsTimeOut { get { return isTimeout; } }
		private void setTimer(int time)
		{
			countTime = 0;
			timeLimit = time;
            isTimeout = false;
			timer = new Timer();
			timer.Tick += new EventHandler(endTimer);
			timer.Interval = 100;
			timer.Enabled = true;
			int start = timeLimit / 100;
			if (isDisp)
			{
				string timeString1 = "残り ";
				string timeString2 = ((double)start / 10.0).ToString();
				PrintLine(timeString1 + timeString2);
			}
		}

		public void ReadIntegerWithTimer(Int64 time, Int64 def_result, Int64 isDisplay, bool isOne, string label)
		{
			result = def_result;
			state = (isOne) ? ConsoleState.WaitOneIntegerWithTimer : ConsoleState.WaitIntegerWithTimer;
			isDisp = isDisplay != 0;
            timeup_label = label;
            if (isOne)
                window.update_lastinput();
			setTimer((int)time);
		}

        public void ReadStringWithTimer(Int64 time, string def_result, Int64 isDisplay, bool isOne, string label)
		{
			results = def_result;
            state = (isOne) ? ConsoleState.WaitOneStringWithTimer : ConsoleState.WaitStringWithTimer;
			isDisp = isDisplay != 0;
            timeup_label = label;
            if (isOne)
                window.update_lastinput();
            setTimer((int)time);
        }

		public void waitInputWithTimer(Int64 time, Int64 flag)
		{
			if (flag == 0)
				state = ConsoleState.WaitKeyWithTimer;
			else
				state = ConsoleState.WaitKeyWithTimerF;
			isDisp = false;
			setTimer((int)time);
		}

		private void stopTimer()
		{
            if (state == ConsoleState.WaitKeyWithTimerF && countTime < timeLimit)
            {
                wait_timeout = true;
                while (countTime < timeLimit)
                {
                    Application.DoEvents();
                }
                wait_timeout = false;
            }
			timer.Enabled = false;
            timer.Dispose();
		}

		private void endTimer(object sender, EventArgs e)
		{
			countTime += 100;
			if (countTime >= timeLimit)
			{
                if (wait_timeout)
                    return;
				stopTimer();
                isTimeout = true;
                if (isDisp)
                    changeLastLine(timeup_label);
                else if (state != ConsoleState.WaitKeyWithTimer && state != ConsoleState.WaitKeyWithTimerF)
                    PrintLine(timeup_label);
				if (state == ConsoleState.WaitIntegerWithTimer || state == ConsoleState.WaitOneIntegerWithTimer)
					state = ConsoleState.Timeout;
				else if (state == ConsoleState.WaitStringWithTimer || state == ConsoleState.WaitOneStringWithTimer)
					state = ConsoleState.Timeouts;
				callEmueraProgram("");
				RefreshStrings(true);
				return;
			}
			if (!isDisp)
				return;
			int time = (timeLimit - countTime) / 100;
			string timeString1 = "残り ";
			string timeString2 = ((double)time / 10.0).ToString();
			changeLastLine(timeString1 + timeString2);
		}

        public void forceStopTimer()
        {
            if (IsRunningTimer && timer.Enabled)
            {
                timer.Stop();
                timer.Dispose();
            }
        }
		#endregion

		#region Call系
		private void callEmueraProgram(string str)
		{
			if (!doInputToEmueraProgram(str))
				return;
            if (state == ConsoleState.Error)
				return;
			state = ConsoleState.Running;
			emuera.DoScript();
			if (state == ConsoleState.Running)
			{//RunningならProcessは処理を継続するべき
				state = ConsoleState.Error;
                PrintError("emuera.exeのエラー：プログラムの状態を特定できません");
			}
			if (state == ConsoleState.Error && !noOutputLog)
				outputLog("emuera.log");
			PrintFlush(false);
			newGeneration();
		}

		private bool doInputToEmueraProgram(string str)
		{
			Int64 input = 0;
			switch (state)
			{
				case ConsoleState.WaitInteger:
                case ConsoleState.WaitOneInteger:
                    if (!Int64.TryParse(str, out input))
                    {
                        if (string.IsNullOrEmpty(str) && hasDefValue)
                            input = defNum;
                        else
                            return false;
                    }
                    hasDefValue = false;
					emuera.InputInteger(input);
					break;
				case ConsoleState.WaitSystemInteger:
					if (!Int64.TryParse(str, out input))
						return false;
					emuera.InputSystemInteger(input);
					break;
				case ConsoleState.WaitString:
                case ConsoleState.WaitOneString:
                    if (string.IsNullOrEmpty(str) && hasDefValue)
                        str = defStr;
                    hasDefValue = false;
                    emuera.InputString(str);
					break;
                case ConsoleState.WaitIntegerWithTimer:
                case ConsoleState.WaitOneIntegerWithTimer:
					if (!Int64.TryParse(str, out input))
						return false;
					stopTimer();
					emuera.InputInteger(input);
					break;
				case ConsoleState.WaitStringWithTimer:
                case ConsoleState.WaitOneStringWithTimer:
					stopTimer();
					emuera.InputString(str);
					break;
				case ConsoleState.Timeout:
					emuera.InputInteger(result);
					str = result.ToString();
					break;
				case ConsoleState.Timeouts:
					emuera.InputString(results);
                    str = results;
					break;
				default:
					break;
			}
			Print(str);
			PrintFlush(false);
			RefreshStrings(false);
			return true;
		}
		#endregion

		#region 入力系
		readonly string[] spliter = new string[] { "\\n", "\r\n", "\n", "\r" };//本物の改行コードが来ることは無いはずだけど一応
        public bool inProcess = false;
        //public bool inMouseSkip = false;
		public bool mesSkip = false;
        private bool isForceWait = false;
		public void PressEnterKey(bool keySkip, string str, ScriptPosition pos, bool changedByMouse)
		{
			mesSkip = keySkip;
			if ((state == ConsoleState.Running) || (state == ConsoleState.Initializing) || (state == ConsoleState.WaitKeyWithTimerF))
				return;
			else if ((state == ConsoleState.Quit))
			{
				window.Close();
				return;
			}
            else if (state == ConsoleState.Error)
            {
				if (str == "openFileWithDebug" && pos != null)
                {
                    ProcessStartInfo pInfo = new ProcessStartInfo();
                    pInfo.FileName = Config.TextEditor;
                    string fname = pos.Filename.ToUpper();
                    if (fname.EndsWith(".CSV"))
                    {
                        if (fname.Contains(Program.CsvDir.ToUpper()))
                            fname = fname.Replace(Program.CsvDir.ToUpper(), "");
                        fname = Program.CsvDir + fname;
                    }
                    else
                    {
                        //解析モードの場合は見ているファイルがERB\の下にあるとは限らないかつフルパスを持っているのでこの補正はしなくてよい
                        if (!Program.AnalysisMode)
                        {
                            if (fname.Contains(Program.ErbDir.ToUpper()))
                                fname = fname.Replace(Program.ErbDir.ToUpper(), "");
                            fname = Program.ErbDir + fname;
                        }
                    }
                    if (Config.EditorArg != "" && Config.EditorArg != null)
                        pInfo.Arguments = Config.EditorArg + pos.LineNo.ToString() + " \"" + fname + "\"";
                    else
                        pInfo.Arguments = fname;
                    try
                    {
                        System.Diagnostics.Process.Start(pInfo);
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        System.Media.SystemSounds.Hand.Play();
                        PrintError("エディタを開くことができませんでした");
                        forceUpdateGeneration();
                    }
                    return;
                }
                else
                {
                    window.Close();
                    return;
                }
            }
			else if (state == ConsoleState.WaitKeyWithTimer)
				stopTimer();
			if (str.StartsWith("@") && !IsWaintingOnePhrase)
			{
				doSystemCommand(str);
				return;
			}
            if (((state == ConsoleState.WaitInteger) || (state == ConsoleState.WaitIntegerWithTimer) || (state == ConsoleState.WaitOneIntegerWithTimer) || (state == ConsoleState.WaitOneInteger) || (state == ConsoleState.WaitSystemInteger)
                || (state == ConsoleState.WaitString) || (state == ConsoleState.WaitStringWithTimer) || (state == ConsoleState.WaitOneStringWithTimer) || (state == ConsoleState.WaitOneString))
                && str.Contains("("))
            {
                StringStream st = new StringStream(str);
                str = parseInput(st, false);
            }
			string[] text = str.Split(spliter, StringSplitOptions.None);
            inProcess = (text.Length > 1);
            for (int i = 0; i < text.Length; i++)
			{
				string inputs = text[i];
                if (inputs.IndexOf("\\e") >= 0)
                {
                    inputs = inputs.Replace("\\e", "");//\eの除去
					mesSkip = true;
                }
                if (IsWaintingOnePhrase && (!Config.AllowLongInputByMouse || !changedByMouse) && inputs.Length > 1)
                    inputs = inputs.Remove(1);
                if (state == ConsoleState.WaitKeyWithTimer || state == ConsoleState.WaitKeyWithTimerF)
                    stopTimer();
                //強制待ちWAITは入力を受け付けないので次に回す必要がある。
                if (state != ConsoleState.WaitKeyWithTimerF)
                    callEmueraProgram(inputs);
                else
                {
                    i--;
                    callEmueraProgram("");
                }
				if (mesSkip)
                {
                    while ((state == ConsoleState.WaitKey && !isForceWait) || state == ConsoleState.WaitKeyWithTimer || state == ConsoleState.WaitKeyWithTimerF || state == ConsoleState.WaitAnyKey)
                    {
                        if (state == ConsoleState.WaitKeyWithTimer || state == ConsoleState.WaitKeyWithTimerF)
                            stopTimer();
                        callEmueraProgram("");
                    }
                }
				mesSkip = false;
                isForceWait = false;
                if (state == ConsoleState.Error || state == ConsoleState.Quit)
                    break;
                //マクロループ時は待ち処理が起こらないのでここでシステムキューを捌く
                Application.DoEvents();
            }
			RefreshStrings(true);
            inProcess = false;
		}

        string parseInput(StringStream st, bool isNest)
        {
            StringBuilder sb = new StringBuilder(20);
            StringBuilder num = new StringBuilder(20);
            int res = 0;
            while (!st.EOS && (!isNest || st.Current != ')'))
            {
                if (st.Current == '(')
                {
                    st.ShiftNext();
                    string tstr = parseInput(st, true);

                    if (!st.EOS)
                    {
                        st.ShiftNext();
                        if (st.Current == '*')
                        {
                            st.ShiftNext();
                            while (char.IsNumber(st.Current))
                            {
                                num.Append(st.Current);
                                st.ShiftNext();
                            }
                            if (num.ToString() != "" && num.ToString() != null)
                            {
                                int.TryParse(num.ToString(), out res);
                                for (int i = 0; i < res; i++)
                                    sb.Append(tstr);
                                num.Remove(0, num.Length);
                            }
                        }
                        else
                            sb.Append(tstr);
                        continue;
                    }
                    else
                    {
                        sb.Append(tstr);
                        break;
                    }
                }
                else if (st.Current == '\\')
                {
                    st.ShiftNext();
                    switch (st.Current)
                    {
                        case 'n':
                            sb.Append('\n');
                            break;
                        case 'r':
                            sb.Append('\r');
                            break;
                        case 'e':
                            sb.Append("\\e");
                            break;
                        case '\n':
                            break;
                        default:
                            sb.Append(st.Current);
                            break;
                    }
                }
                else
                    sb.Append(st.Current);
                st.ShiftNext();
            }
            return sb.ToString();
        }

		bool isDebug = false;
		/// <summary>
		/// 通常コンソールからのDebugコマンド、及びデバッグウインドウの変数ウォッチなど、
		/// *.ERBファイルが存在しないスクリプトを実行中
		/// 1750 IsDebugから改名
		/// </summary>
		public bool RunERBFromMemory { get { return isDebug; } set { isDebug = value; } }
		void doSystemCommand(string command)
		{
			StringComparison sc = Config.SCVariable;
			Print(command);
			PrintFlush(false);
			RefreshStrings(true);
			string com = command.Substring(1);
			if (com.Length == 0)
				return;
			if (com.Equals("REBOOT", sc))
			{
				window.Reboot();
				return;
			}
			else if (com.Equals("OUTPUT", sc) || com.Equals("OUTPUTLOG", sc))
			{
				this.outputLog("emuera.log");
				return;
			}
			else if ((com.Equals("QUIT", sc)) || (com.Equals("EXIT", sc)))
			{
				window.Close();
				return;
			}
			else if (com.Equals("CONFIG", sc))
			{
				window.ShowConfigDialog();
				return;
			}
			else if (com.Equals("DEBUG", sc))
			{
				if (!Program.DebugMode)
				{
					PrintError("デバッグウインドウは-Debug引数付きで起動したときのみ使えます");
					RefreshStrings(true);
					return;
				}
				OpenDebugDialog();				
			}
			else
			{
				if (!Config.UseDebugCommand)
				{
					PrintError("デバッグコマンドを使用できない設定になっています");
					RefreshStrings(true);
					return;
				}
				//処理をDebugMode系へ移動
				DebugCommand(com, Config.ChangeMasterNameIfDebug, false);
				PrintFlush(false);
			}
			RefreshStrings(true);
		}
		#endregion

		#region 描画系
		uint lastUpdate = 0;
		uint msPerFrame = 1000 / 60;//60FPS
		ConsoleRedraw redraw = ConsoleRedraw.Normal;
        public ConsoleRedraw Redraw { get { return redraw; } }
		public void SetRedraw(Int64 i)
		{
			if ((i & 1) == 0)
				redraw = ConsoleRedraw.None;
			else
				redraw = ConsoleRedraw.Normal;
			if ((i & 2) != 0)
				RefreshStrings(true);
		}

		string debugTitle = null;
		public void SetWindowTitle(string str)
		{
			if (Program.DebugMode)
			{
				debugTitle = str;
				window.Text = str + " (Debug Mode)";
			}
			else
				window.Text = str;
		}

		public string GetWindowTitle()
		{
			if (Program.DebugMode && debugTitle != null)
				return debugTitle;
			return window.Text;
		}

        public void rePaint(Graphics g)
        {
            if (!Config.UseImageBuffer)
            {
                g.Clear(this.bgColor);
                RefreshStrings(true, g);
            }
        }
		public void RefreshStrings(bool force)
		{
			RefreshStrings(force, null);
		}
		public void RefreshStrings(bool force, Graphics graph)
		{
			//描画中にEmueraが閉じられると廃棄されたPictureBoxにアクセスしてしまったりするので
			if (!this.Enabled)
				return;
			bool isBackLog = window.ScrollBar.Value != window.ScrollBar.Maximum;
            //ログ表示はREDRAWの設定に関係なく行うようにする
            if ((redraw == ConsoleRedraw.None) && (!force) && (!isBackLog))
                return;
            if (selectingButton != null)
			{
				//履歴表示中は選択肢無効
				//if (isBackLog)
				//	selectingButton = null;
				//数値か文字列の入力待ち状態でなければ無効
				if ((state != ConsoleState.WaitInteger) && (state != ConsoleState.WaitString)
				&& (state != ConsoleState.WaitSystemInteger) && (state != ConsoleState.WaitIntegerWithTimer)
				&& (state != ConsoleState.WaitStringWithTimer) && (state != ConsoleState.Error)
                && (state != ConsoleState.WaitOneInteger) && (state != ConsoleState.WaitOneString)
                && (state != ConsoleState.WaitOneIntegerWithTimer) && (state != ConsoleState.WaitOneStringWithTimer))
					selectingButton = null;
				//選択肢が最新でないなら無効
				else if (selectingButton.Generation != lastButtonGeneration)
					selectingButton = null;
			}
			if (!force)
			{//forceならば確実に再描画。
				if ((!isBackLog) && (lastDrawnLineNo == lineNo) && (lastSelectingButton == selectingButton))
					return;
				//Environment.TickCountは分解能が悪すぎるのでwinmmのタイマーを呼んで来る
				uint sec = WinmmTimer.TickCount - lastUpdate;
				//まだ書き換えるタイミングでないなら次の更新を待ってみる
				//ただし、入力待ちなど、しばらく更新のタイミングがない場合には強制的に書き換えてみる
				if (sec < msPerFrame && (state == ConsoleState.Running || state == ConsoleState.Initializing))
					return;
			}
            if (forceTextBoxColor)
            {
                uint sec = WinmmTimer.TickCount - lastBgColorChange;
                //色変化が速くなりすぎないように一定時間以内の再呼び出しは強制待ちにする
                while (sec < 200)
                {
                    Application.DoEvents();
                    sec = WinmmTimer.TickCount - lastBgColorChange;
                }
                window.TextBox.BackColor = this.bgColor;
                lastBgColorChange = WinmmTimer.TickCount;
            }
            lastUpdate = WinmmTimer.TickCount;
			barUpdate();
			window.SuspendLayout();
			this.m_RefreshStrings(graph);
			window.ResumeLayout();
            if (Config.UseImageBuffer)
				window.Refresh();
			if (isBackLog)
				lastDrawnLineNo = -1;
			else
				lastDrawnLineNo = lineNo;
			lastSelectingButton = selectingButton;
			/*デバッグ用。描画が超重い環境を想定
			System.Threading.Thread.Sleep(50);
			*/
			//Initializing中はさらに頻度を下げる：読み込みを早くするため。
            if (state == ConsoleState.Initializing)
                lastUpdate = WinmmTimer.TickCount;
            if (forceTextBoxColor)
            {
                forceTextBoxColor = false;
            }
		}

		private void m_RefreshStrings(Graphics g)
		{
			if (!this.Enabled)
				return;
			bool isBackLog = window.ScrollBar.Value != window.ScrollBar.Maximum;
			bool areaRefresh = false;
            if (Config.UseImageBuffer)
				areaRefresh = ((!isBackLog) && (lastDrawnLineNo > 0) && ((lineNo - lastDrawnLineNo - 1) < window.MainPicBox.Height / Config.LineHeight));
			int pointY = window.MainPicBox.Height - Config.LineHeight;
            //if (areaRefresh)
            //{
            //    int newLineNum = lineNo - lastDrawnLineNo;
            //    if (newLineNum != 0)
            //    {
            //        shiftBitmap((Bitmap)window.MainPicBox.Image, newLineNum * StaticConfig.LineHeight);
            //    }
            //}
			bool onpaint = (g != null);

            BufferedGraphicsContext bufContext = null;
            BufferedGraphics bufGraph = null;
            //Graphics graph = g;
            //if (graph == null)
            //    graph = getGraphics();
            if (g == null)
                g = getGraphics();
            //if (graph == null)
			//	return;

            //bufContext = BufferedGraphicsManager.Current;

            Graphics graph = g;
			//graph.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
			//graph.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;
            if (Config.TextDrawingMode == TextDrawingMode.WINAPI)
			{
                if (!Config.UseImageBuffer)
                {
                    bufContext = new BufferedGraphicsContext();
                    bufContext.MaximumBuffer = new Size(window.MainPicBox.Width + 1, window.MainPicBox.Height + 1);
                    bufGraph = bufContext.Allocate(g, new Rectangle(0, 0, window.MainPicBox.Width, window.MainPicBox.Height));
                    graph = bufGraph.Graphics;
                }
                GDI.GDIStart(graph, this.bgColor);
                //GDI.GDIStart(bufGraph.Graphics, StaticConfig.BackColor);
                //graph = bufGraph.Graphics;
			}
            if (areaRefresh && !bgColorChanged && !forceTextBoxColor)
			{//元画面を上に移動
				int newLineNum = lineNo - lastDrawnLineNo;
                if (newLineNum != 0)
                {
                    shiftBitmap((Bitmap)window.MainPicBox.Image, newLineNum * Config.LineHeight);
                    for (int i = window.ScrollBar.Value - 1; i >= window.ScrollBar.Value - newLineNum; i--)
                    {
                        drawDisplayLine(displayLineList[i], graph, pointY, isBackLog, true);
                        pointY -= Config.LineHeight;
                        if (pointY < -Config.LineHeight)
                            break;
                    }
                }
				if (lastSelectingButton != selectingButton)
				{
					ConsoleDisplayLine target = null;
					if (lastSelectingButton != null)
					{
						target = lastSelectingButton.ParentLine;
						int pointCount = displayLineList[window.ScrollBar.Value - 1].LineNo - target.LineNo;
						pointY = window.MainPicBox.Height - Config.LineHeight - pointCount * Config.LineHeight;
                        drawDisplayLine(target, graph, pointY, isBackLog, true);
					}
					if ((selectingButton != null) &&
					!((lastSelectingButton != null) && (lastSelectingButton.ParentLine == selectingButton.ParentLine)))
					{
						target = selectingButton.ParentLine;
						int pointCount = displayLineList[window.ScrollBar.Value - 1].LineNo - target.LineNo;
						pointY = window.MainPicBox.Height - Config.LineHeight - pointCount * Config.LineHeight;
                        drawDisplayLine(target, graph, pointY, isBackLog, true);
					}
				}
			}
			else
			{//全描画
                if ((Config.TextDrawingMode != TextDrawingMode.WINAPI) && (Config.UseImageBuffer))
                {//WINAPIでなく、ImageBufferを使うなら全クリアしてもよい。使わないなら全クリアするとひどくちらつく。
                    graph.Clear(this.bgColor);
                }
                else if ((window.ScrollBar.Value * Config.LineHeight) < (window.MainPicBox.Height))
                {//文字描画するところは描画の際に綺麗にする。
                //文字が描画されない領域があるとき、そこをきれいにしておく必要がある
                    int rectBottom = window.MainPicBox.Height - (window.ScrollBar.Value * Config.LineHeight);
                    Rectangle rect = new Rectangle(0, 0, window.MainPicBox.Width, rectBottom);
                    if (Config.TextDrawingMode == TextDrawingMode.WINAPI)
                    {
                        GDI.FillRect(rect);
                    }
                    else
                    {
                        graph.FillRectangle(new SolidBrush(this.bgColor), rect);
                    }
                }
				for (int i = window.ScrollBar.Value - 1; i >= 0; i--)
				{
                    drawDisplayLine(displayLineList[i], graph, pointY, isBackLog, !Config.UseImageBuffer);
					pointY -= Config.LineHeight;
					if (pointY < 0 - Config.LineHeight)
						break;
				}

			}
            if (Config.TextDrawingMode == TextDrawingMode.WINAPI)
			{
                GDI.GDIEnd(graph);
                if (!Config.UseImageBuffer)
                {
                    bufGraph.Render();
                    bufGraph.Dispose();
                    bufContext.Dispose();
                }
            }
            if (!onpaint)
            {
                graph.Dispose();
                g.Dispose();
            }
			//ToolTip描画

			if (lastPointingString != pointingString)
			{
				if(tooltipUsed)
					window.ToolTip.RemoveAll();
				if (pointingString != null && !string.IsNullOrEmpty(pointingString.Title))
				{
					window.ToolTip.SetToolTip(window.MainPicBox, pointingString.Title);
					tooltipUsed = true;
				}
				lastPointingString = pointingString;
			}
		}

		public void SetToolTipColor(Color foreColor, Color backColor)
		{
			window.ToolTip.ForeColor = foreColor;
			window.ToolTip.BackColor = backColor;

		}
		public void SetToolTipDelay(int delay)
		{
			window.ToolTip.InitialDelay = delay;
		}

		void drawDisplayLine(ConsoleDisplayLine line, Graphics g, int pointY, bool isBackLog, bool clear)
		{
            if (Config.TextDrawingMode == TextDrawingMode.WINAPI)
			{
				//line.GDIClear(pointY);
				line.GDIDrawTo(pointY, isBackLog);
			}
			else
			{
				if (clear)
                    line.Clear(new SolidBrush(this.bgColor), g, pointY);
				line.DrawTo(g, pointY, isBackLog, true, Config.TextDrawingMode);
			}
		}

		private Graphics getGraphics()
		{
            //消したいが怖いので残し
            if (!window.Created)
                throw new ExeEE("存在しないウィンドウにアクセスした");
            if (Config.UseImageBuffer)
				return Graphics.FromImage(window.MainPicBox.Image);
			else
				return window.MainPicBox.CreateGraphics();
		}

		int[] rgbValues;
		private void shiftBitmap(Bitmap bmp, int shiftY)
		{
			Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
			System.Drawing.Imaging.BitmapData bmpData =
			bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
			IntPtr ptr = bmpData.Scan0;
			int lineBytes = bmp.Width;
			int totalBytes = lineBytes * bmp.Height;
			System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, totalBytes);
			System.Runtime.InteropServices.Marshal.Copy(rgbValues, lineBytes * shiftY, ptr, totalBytes);
			bmp.UnlockBits(bmpData);
		}
		#endregion

		#region Print系
		private readonly List<ConsoleDisplayLine> displayLineList;
		
		private bool useUserStyle = true;
		public bool UseUserStyle { get { return useUserStyle; } set { useUserStyle = value; } }
		public bool UseSetColorStyle { get; set; }
		private StringStyle defaultStyle = new StringStyle(Config.ForeColor, FontStyle.Regular, null);
        private StringStyle userStyle = new StringStyle(Config.ForeColor, FontStyle.Regular, null);
        private StringStyle style = new StringStyle(Config.ForeColor, FontStyle.Regular, null);
		private StringStyle Style
		{
			get
			{
				if (!useUserStyle)
					return defaultStyle;
				if (UseSetColorStyle)
					return userStyle;
				//PRINTD系(SETCOLORを無視する)
				if (userStyle.Color == defaultStyle.Color)
					return userStyle;
				return new StringStyle(defaultStyle.Color, userStyle.FontStyle, userStyle.Fontname);
			}
		}
		//private StringStyle Style { get { return (useUserStyle ? userStyle : defaultStyle); } }
		public StringStyle StringStyle { get { return userStyle; } }
		public void SetStringStyle(FontStyle fs) { userStyle.FontStyle = fs; }
		public void SetStringStyle(Color color) { userStyle.Color = color; }
		public void SetFont(string fontname) { userStyle.Fontname = fontname; }
		private DisplayLineAlignment alignment = DisplayLineAlignment.LEFT;
		public DisplayLineAlignment Alignment { get { return alignment; } set { alignment = value; } }
		public void ResetStyle()
		{
			style = defaultStyle;
			alignment = DisplayLineAlignment.LEFT;
		}

        public bool EmptyLine { get { return printBuffer.IsEmpty; } }
		string stBar = null;

        uint lastBgColorChange = 0;
        bool bgColorChanged = false;
        bool forceTextBoxColor = false;
        public void SetBgColor(Color color)
        {
            this.bgColor = color;
            forceTextBoxColor = true;
            //REDRAWされない場合はTextBoxの色は変えずにフラグだけ立てる
            //最初の再描画時に現在の背景色に合わせる
            if (redraw == ConsoleRedraw.None && window.ScrollBar.Value == window.ScrollBar.Maximum)
                return;
            //この場合は必ず全部書き換える
            bgColorChanged = true;
            uint sec = WinmmTimer.TickCount - lastBgColorChange;
            //色変化が速くなりすぎないように一定時間以内の再呼び出しは強制待ちにする
            while (sec < 200)
            {
                Application.DoEvents();
                sec = WinmmTimer.TickCount - lastBgColorChange;
            }
            RefreshStrings(true);
            lastBgColorChange = WinmmTimer.TickCount;
            bgColorChanged = false;
        }

		/// <summary>
		/// 最後に描画した時にlineNoの値。次に全描画を必要とする場合には-１。
		/// </summary>
		int lastDrawnLineNo = -1;
		int lineNo = 0;
        Int64 logicalLineCount = 0;
		public long LineCount { get { return logicalLineCount; } }
		private void addRangeDisplayLine(ConsoleDisplayLine[] lineList)
		{
			for (int i = 0; i < lineList.Length; i++)
				addDisplayLine(lineList[i]);
		}

		private void addDisplayLine(ConsoleDisplayLine line)
		{
			if (LastLineIsTemporary)
				deleteLine(1);
			line.SetAlignment(alignment);
			line.LineNo = lineNo;
			displayLineList.Add(line);
			lineNo++;
            if (line.IsLogicalLine)
                logicalLineCount++;
			if (lineNo == int.MaxValue)
			{
				lastDrawnLineNo = -1;
				lineNo = 0;
			}
            if (logicalLineCount == long.MaxValue)
            {
                logicalLineCount = 0;
            }
            if (displayLineList.Count > Config.MaxLog)
				displayLineList.RemoveAt(0);
		}


		public void deleteLine(int argNum)
		{
			int delNum = 0;
			int num = argNum;
			while (delNum < num)
			{
				if (displayLineList.Count == 0)
					break;
				ConsoleDisplayLine line = displayLineList[displayLineList.Count - 1];
				displayLineList.RemoveAt(displayLineList.Count - 1);
				lineNo--;
                if (line.IsLogicalLine)
                {
                    delNum++;
                    logicalLineCount--;
                }
			}
			if (lineNo < 0)
				lineNo += int.MaxValue;
			lastDrawnLineNo = -1;
			//RefreshStrings(true);
		}

		public bool LastLineIsTemporary
		{
			get
			{
				if (displayLineList.Count == 0)
					return false;
				return displayLineList[displayLineList.Count - 1].IsTemporary;
			}
		}

		//最終行を書き換え＋次の行追加時にはその行を再利用するように設定
		public void PrintTemporaryLine(string str)
		{
			PrintSingleLine(str, false, true);
		}

		//最終行だけを書き換える
		private void changeLastLine(string str)
		{
			deleteLine(1);
			PrintSingleLine(str, false, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="str"></param>
		/// <param name="position"></param>
		/// <param name="level">警告レベル.0:軽微なミス.1:無視できる行.2:行が実行されなければ無害.3:致命的</param>
		public void PrintWarning(string str, ScriptPosition position, int level)
		{
            if (level < Config.DisplayWarningLevel && !Program.AnalysisMode)
				return;
			//警告だけは強制表示
			bool b = force_temporary;
			force_temporary = false;
			if (position != null)
			{
				if(position.LineNo >= 0)
				{
					PrintErrorButton(string.Format("警告Lv{0}:{1}:{2}行目:{3}", level, position.Filename, position.LineNo, str), "openFileWithDebug", position);
					if (position.RowLine != null)
						PrintError(position.RowLine);
				}
				else
					PrintErrorButton(string.Format("警告Lv{0}:{1}:{2}", level, position.Filename, str), "openFileWithDebug", position);
				
			}
			else
			{
                PrintError(string.Format("警告Lv{0}:{1}", level, str));
			}
			force_temporary = b;
		}

		/// <summary>
		/// ウィンドウサイズを考慮せず確実に一行で書くかわりに（ちょっとだけ）高速。システム用。
		/// </summary>
		/// <param name="str"></param>
		public void PrintLine(string str) { PrintSingleLine(str, false, false); }
        //public void PrintLine(string str, bool force) 
        //{
        //    bool b = force_temporary;
        //    if (force_temporary)
        //        force_temporary = false;
        //    PrintSingleLine(str, false, false);
        //    force_temporary = b;
        //}
		public void PrintError(string str)
		{
			if (string.IsNullOrEmpty(str))
				return;
			if (Program.DebugMode)
			{
				this.DebugPrint(str);
				this.DebugNewLine();
			}
            PrintFlush(false);
            ConsoleDisplayLine dispLine = PrintPlainwithSingleLine(str);
            if (dispLine == null)
                return;
            DisplayLineAlignment curAlignment = alignment;
            alignment = DisplayLineAlignment.LEFT;
            addDisplayLine(dispLine);
            alignment = curAlignment;
            RefreshStrings(false);
            //printBuffer.Append(str, defaultStyle);
            //ConsoleDisplayLine dispLine = BufferToSingleLine(true, false);
            //if (dispLine == null)
            //    return;
            //DisplayLineAlignment curAlignment = alignment;
            //addDisplayLine(dispLine);
            //alignment = curAlignment;
            //RefreshStrings(false);
		}

		internal void PrintErrorButton(string str, string p, ScriptPosition pos)
		{
			if (string.IsNullOrEmpty(str))
				return;
            ConsoleDisplayLine dispLine = printBuffer.AppendErrButton(str, Style, p, pos, new StringMeasure(getGraphics(), this.bgColor, Config.TextDrawingMode));
            if (dispLine == null)
                return;
            DisplayLineAlignment curAlignment = alignment;
            alignment = DisplayLineAlignment.LEFT;
            addDisplayLine(dispLine);
            alignment = curAlignment;
            RefreshStrings(false);
            if (Program.DebugMode)
			{
				this.DebugPrint(str);
				this.DebugNewLine();
			}
		}

		/// <summary>
		/// 必ず一行で中央に描画する。タイトル表示用。
		/// </summary>
		/// <param name="str"></param>
		public void PrintCenter(string str) { PrintSingleLine(str, true, false); }
		public void PrintSingleLine(string str, bool center, bool temporary)
		{
			if (string.IsNullOrEmpty(str))
				return;
			PrintFlush(false);
			printBuffer.Append(str, Style);
            ConsoleDisplayLine dispLine = BufferToSingleLine(true, temporary);
			if (dispLine == null)
				return;
			DisplayLineAlignment curAlignment = alignment;
			if (center)
				alignment = DisplayLineAlignment.CENTER;
			addDisplayLine(dispLine);
			alignment = curAlignment;
			RefreshStrings(false);
		}

		public void Print(string str)
		{
			if (string.IsNullOrEmpty(str))
				return;
			if (str.Contains("\n"))
			{
                int newline = str.IndexOf('\n');
                string upper = str.Substring(0, newline);
				printBuffer.Append(upper, Style);
				NewLine();
                if (newline < str.Length - 1)
                {
                    string lower = str.Substring(newline + 1);
                    Print(lower);
                }
				return;
			}
			printBuffer.Append(str, Style);
			return;
		}

		public void PrintHtml(string str)
		{
			if (string.IsNullOrEmpty(str))
				return;
			if (!this.Enabled)
				return;
            StringMeasure stringMeasure = new StringMeasure(getGraphics(), this.bgColor, Config.TextDrawingMode);
			if (!printBuffer.IsEmpty)
			{
				ConsoleDisplayLine[] dispList = printBuffer.Flush(stringMeasure, force_temporary);
				addRangeDisplayLine(dispList);
			}
			addRangeDisplayLine(HtmlManager.Html2DisplayLine(str, stringMeasure, this));
			stringMeasure.Dispose();
			RefreshStrings(false);
		}

        private int printCWidth = -1;
        private int printCWidthL = -1;
        private int printCWidthL2 = -1;
        public void PrintC(string str, bool alignmentRight)
		{
			if (string.IsNullOrEmpty(str))
				return;
            
			printBuffer.Append(CreateTypeCString(str, alignmentRight), Style, true);
		}

        private void calcPrintCWidth(StringMeasure stringMeasure)
        {
            string str = new string(' ', Config.PrintCLength);
            printCWidth = stringMeasure.GetDisplayLength(str, Config.Font);
        
            str += " ";
            printCWidthL = stringMeasure.GetDisplayLength(str, Config.Font);

            str += " ";
            printCWidthL2 = stringMeasure.GetDisplayLength(str, Config.Font);
        }

        private string CreateTypeCString(string str, bool alignmentRight)
        {
            StringMeasure stringMeasure = new StringMeasure(getGraphics(), this.bgColor, Config.TextDrawingMode);
            if (printCWidth == -1)
                calcPrintCWidth(stringMeasure);
            int length = 0;
            int width = 0;
            if (str != null)
                length = Config.Encode.GetByteCount(str);
            int printcLength = Config.PrintCLength;
            Font font = new Font(Style.Fontname, Config.Font.Size, Style.FontStyle, GraphicsUnit.Pixel);

            if ((alignmentRight) && (length < printcLength))
            {
                str = new string(' ', printcLength - length) + str;
                width = stringMeasure.GetDisplayLength(str, font);
                while (width > printCWidth)
                {
                    if (str[0] != ' ')
                        break;
                    str = str.Remove(0, 1);
                    width = stringMeasure.GetDisplayLength(str, font);
                }
            }
            else if ((!alignmentRight) && (length < printcLength + 1))
            {
                str += new string(' ', printcLength + 1 - length);
                width = stringMeasure.GetDisplayLength(str, font);
                while (width > printCWidthL)
                {
                    if (str[str.Length - 1] != ' ')
                        break;
                    str = str.Remove(str.Length - 1, 1);
                    width = stringMeasure.GetDisplayLength(str, font);
                }
            }
            stringMeasure.Dispose();
            return str;
        }

		internal void PrintButton(string str, string p)
		{
			if (string.IsNullOrEmpty(str))
				return;
			printBuffer.AppendButton(str, Style, p);
		}
		internal void PrintButton(string str, long p)
		{
			if (string.IsNullOrEmpty(str))
				return;
			printBuffer.AppendButton(str, Style, p);
		}
        internal void PrintButtonC(string str, string p, bool isRight)
        {
            if (string.IsNullOrEmpty(str))
                return;
            printBuffer.AppendButton(CreateTypeCString(str, isRight), Style, p);
        }
        internal void PrintButtonC(string str, long p, bool isRight)
        {
            if (string.IsNullOrEmpty(str))
                return;
            printBuffer.AppendButton(CreateTypeCString(str, isRight), Style, p);
        }

        internal void PrintPlain(string str)
        {
            if (string.IsNullOrEmpty(str))
                return;
            printBuffer.AppendPlainText(str, Style);
        }

		public void NewLine() { PrintFlush(true); }

		public ConsoleDisplayLine BufferToSingleLine(bool force, bool temporary)
		{
			if (!this.Enabled)
				return null;
			if (!force && printBuffer.IsEmpty)
				return null;
			if (force && printBuffer.IsEmpty)
				printBuffer.Append(" ", Style);
            StringMeasure stringMeasure = new StringMeasure(getGraphics(), this.bgColor, Config.TextDrawingMode);
			ConsoleDisplayLine dispLine = printBuffer.FlushSingleLine(stringMeasure, temporary | force_temporary);
            stringMeasure.Dispose();
			return dispLine;
		}

        internal ConsoleDisplayLine PrintPlainwithSingleLine(string str)
        {
            if (!this.Enabled)
                return null;
            if (string.IsNullOrEmpty(str))
                return null;
            printBuffer.AppendPlainText(str, Style);
            StringMeasure stringMeasure = new StringMeasure(getGraphics(), this.bgColor, Config.TextDrawingMode);
            ConsoleDisplayLine dispLine = printBuffer.FlushSingleLine(stringMeasure, false);
            stringMeasure.Dispose();
            return dispLine;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="force">バッファーが空のとき改行する</param>
		public void PrintFlush(bool force) //{ PrintFlush(force, false); }
        //public void PrintFlush(bool force, bool temporary)
		{
			if (!this.Enabled)
				return;
			if (!force && printBuffer.IsEmpty)
				return;
			if (force && printBuffer.IsEmpty)
				printBuffer.Append(" ", Style);

            StringMeasure stringMeasure = new StringMeasure(getGraphics(), this.bgColor, Config.TextDrawingMode);
            ConsoleDisplayLine[] dispList = printBuffer.Flush(stringMeasure, force_temporary);
            //ConsoleDisplayLine[] dispList = printBuffer.Flush(stringMeasure, temporary | force_temporary);
			stringMeasure.Dispose();
			addRangeDisplayLine(dispList);
			RefreshStrings(false);
		}

		/// <summary>
		/// DRAWLINE命令に対応。これのフォントを変更できると面倒なことになるのでRegularに固定する。
		/// </summary>
		public void PrintBar()
		{
            //初期に設定済みなので見る必要なし
            //if (stBar == null)
            //    setStBar(StaticConfig.DrawLineString);

			//1806beta001 CompatiDRAWLINEの廃止、CompatiLinefeedAs1739へ移行
			//CompatiLinefeedAs1739の処理はPrintStringBuffer.csで行う
			//if (Config.CompatiDRAWLINE)
			//	PrintFlush(false);
            StringStyle ss = userStyle;
            userStyle.FontStyle = FontStyle.Regular;
            Print(stBar);
            userStyle = ss;
		}

        public void printCustomBar(string barStr)
        {
            StringStyle ss = userStyle;
            userStyle.FontStyle = FontStyle.Regular;
            Print(getStBar(barStr));
            userStyle = ss;
        }

        public string getDefStBar()
        {
            return stBar;
        }

        public string getStBar(string barStr)
        {
            StringMeasure stringMeasure = new StringMeasure(getGraphics(), this.bgColor, Config.TextDrawingMode);
            StringBuilder bar = new StringBuilder();
            bar.Append(barStr);
            int width = 0;
            while (width < window.MainPicBox.Width)
            {//境界を越えるまで一文字ずつ増やす
                bar.Append(barStr);
                width = stringMeasure.GetDisplayLength(bar.ToString(), Config.Font);
            }
            while (width > window.MainPicBox.Width)
            {//境界を越えたら、今度は超えなくなるまで一文字ずつ減らす（barStrに複数字の文字列がきた場合に対応するため）
                bar.Remove(bar.Length - 1, 1);
                width = stringMeasure.GetDisplayLength(bar.ToString(), Config.Font);
            }
            stringMeasure.Dispose();
            return bar.ToString();
        }

		public void setStBar(string barStr)
		{
			stBar = getStBar(barStr);
		}
		#endregion

		#region DebugMode系
		DebugDialog dd = null;
		public DebugDialog DebugDialog { get { return dd; } }
		StringBuilder dConsoleLog = new StringBuilder("");
		public string DebugConsoleLog { get { return dConsoleLog.ToString(); } }
		List<string> dTraceLogList = new List<string>();
		bool dTraceLogChanged = true;
		public string GetDebugTraceLog(bool force)
		{
			if (!dTraceLogChanged && !force)
				return null;
			StringBuilder builder = new StringBuilder("");
			for (int i = dTraceLogList.Count - 1; i >= 0; i--)
			{
				builder.AppendLine(dTraceLogList[i]);
			}
			return builder.ToString();
		}
		public void OpenDebugDialog()
		{
			if (!Program.DebugMode)
				return;
			if (dd != null)
			{
				if (dd.Created)
				{
					dd.Focus();
					return;
				}
				else
				{
					dd.Dispose();
					dd = null;
				}
			}
			dd = new DebugDialog();
			dd.SetParent(this, emuera);
			dd.Show();
		}

		public void DebugPrint(string str)
		{
			if (!Program.DebugMode)
				return;
			dConsoleLog.Append(str);
		}

		public void DebugClear()
		{
			dConsoleLog.Remove(0, dConsoleLog.Length);
		}

		public void DebugNewLine()
		{
			if (!Program.DebugMode)
				return;
			dConsoleLog.Append(Environment.NewLine);
		}

		public void DebugAddTraceLog(string str)
		{
			//Emueraがデバッグモードで起動されていないなら無視
			//ERBファイル以外のもの(デバッグコマンド、変数ウォッチ)を実行中なら無視
			if (!Program.DebugMode || isDebug)
				return;
			dTraceLogChanged = true;
			dTraceLogList.Add(str);
		}
		public void DebugRemoveTraceLog()
		{
			if (!Program.DebugMode || isDebug)
				return;
			dTraceLogChanged = true;
			if(dTraceLogList.Count > 0)
				dTraceLogList.RemoveAt(dTraceLogList.Count-1);
		}
		public void DebugClearTraceLog()
		{
			if (!Program.DebugMode || isDebug)
				return;
			dTraceLogChanged = true;
			dTraceLogList.Clear();
		}

		public void DebugCommand(string com, bool munchkin, bool outputDebugConsole)
		{
			ConsoleState temp_state = state;
			isDebug = true;
            //スクリプト等が失敗した場合に備えて念のための保存
            GlobalStatic.Process.saveCurrentState(false);
            try
			{
				LogicalLine line = null;
				if (!com.StartsWith("@") && !com.StartsWith("\"") && !com.StartsWith("\\"))
					line = LogicalLineParser.ParseLine(com, null);
				if (line == null || (line is InvalidLine))
				{
					WordCollection wc = LexicalAnalyzer.Analyse(new StringStream(com), LexEndWith.EoL, LexAnalyzeFlag.None);
					IOperandTerm term = ExpressionParser.ReduceExpressionTerm(wc, TermEndWith.EoL);
					if (term == null)
						throw new CodeEE("解釈不能なコードです");
					if (term.GetOperandType() == typeof(Int64))
					{
						if (outputDebugConsole)
							com = "DEBUGPRINTFORML {" + com + "}";
						else
							com = "PRINTVL " + com;
					}
					else
					{
						if (outputDebugConsole)
							com = "DEBUGPRINTFORML %" + com + "%";
						else
							com = "PRINTFORMSL " + com;
					}
					line = LogicalLineParser.ParseLine(com, null);
				}
				if (line == null)
					throw new CodeEE("解釈不能なコードです");
				if (line is InvalidLine)
					throw new CodeEE(line.ErrMes);
				if (!(line is InstructionLine))
					throw new CodeEE("デバッグコマンドで使用できるのは代入文か命令文だけです");
				InstructionLine func = (InstructionLine)line;
				if (func.Function.IsFlowContorol())
					throw new CodeEE("フロー制御命令は使用できません");
				//__METHOD_SAFE__をみるならいらないかも
				if (func.Function.IsWaitInput())
					throw new CodeEE(func.Function.Name + "命令は使用できません");
				//1750 __METHOD_SAFE__とほぼ条件同じだよねってことで
				if (!func.Function.IsMethodSafe())
					throw new CodeEE(func.Function.Name + "命令は使用できません");
				//1756 SIFの次に来てはいけないものはここでも不可。
				if (func.Function.IsPartial())
					throw new CodeEE(func.Function.Name + "命令は使用できません");
				switch (func.FunctionCode)
				{//取りこぼし
					//逆にOUTPUTLOG、QUITはDebugCommandの前に捕まえる
					case FunctionCode.PUTFORM:
					case FunctionCode.UPCHECK:
					case FunctionCode.CUPCHECK:
					case FunctionCode.SAVEDATA:
						throw new CodeEE(func.Function.Name + "命令は使用できません");
				}
				ArgumentParser.SetArgumentTo(func);
				if (func.IsError)
					throw new CodeEE(func.ErrMes);
				emuera.DoDebugNormalFunction(func, munchkin);
				if (func.FunctionCode == FunctionCode.SET)
				{
					if (!outputDebugConsole)
						PrintLine(com);
					//DebugWindowのほうは少しくどくなるのでいらないかな
				}
			}
			catch (Exception e)
			{
				if (outputDebugConsole)
				{
					DebugPrint(e.Message);
					DebugNewLine();
				}
				else
					PrintError(e.Message);
				emuera.clearMethodStack();
			}
			finally
			{
                //確実に元の状態に戻す
                GlobalStatic.Process.loadPrevState();
                isDebug = false;
				state = temp_state;
			}
		}
		#endregion

		#region Window.Form系
		public void MoveMouse(Point point)
		{
			ConsoleButtonString select = null;
			ConsoleButtonString pointing = null;
			bool canSelect = true;
			//数値か文字列の入力待ち状態でなければ選択中にはならない
			if ((state != ConsoleState.WaitInteger) && (state != ConsoleState.WaitString)
			&& (state != ConsoleState.WaitSystemInteger) && (state != ConsoleState.WaitIntegerWithTimer)
			&& (state != ConsoleState.WaitStringWithTimer) && (state != ConsoleState.Error)
			&& (state != ConsoleState.WaitOneInteger) && (state != ConsoleState.WaitOneString)
			&& (state != ConsoleState.WaitOneIntegerWithTimer) && (state != ConsoleState.WaitOneStringWithTimer))
				canSelect = false;

			//スクリプト実行中は無視
			if (state == ConsoleState.Running || state == ConsoleState.Initializing)
				goto end;
			//入力・マクロ処理中は無視
			if (inProcess)
				goto end;
			//履歴表示中は無視
			//if (window.ScrollBar.Value != window.ScrollBar.Maximum)
			//	goto end;
			int pointX = point.X;
			int pointY = point.Y;
			//表示上は下から何行目？
			int displayLineNo = (window.MainPicBox.Height - pointY) / Config.LineHeight + 1;
			//実際は何行目？
			//int lineNo = displayLineList.Count - displayLineNo;
			int lineNo = displayLineList.Count - displayLineNo - (window.ScrollBar.Maximum - window.ScrollBar.Value);
			if ((lineNo < 0) || (lineNo >= displayLineList.Count))
				goto end;
			pointing = displayLineList[lineNo].GetPointingButton(pointX);
			if ((pointing == null) || (pointing.Generation != lastButtonGeneration))
				canSelect = false;
			else if (!pointing.IsButton)
				canSelect = false;
			else if ((state == ConsoleState.WaitInteger || state == ConsoleState.WaitIntegerWithTimer || state == ConsoleState.WaitOneInteger || state == ConsoleState.WaitOneIntegerWithTimer) && (!pointing.IsInteger))
				canSelect = false;
			else if ((state == ConsoleState.WaitSystemInteger) && (!pointing.IsInteger))
				canSelect = false;
		end:
			if (canSelect)
				select = pointing;
			bool needRefresh = select != selectingButton || pointing != pointingString;
			pointingString = pointing;
			selectingButton = select;
			if (needRefresh)
			{
				RefreshStrings(true);
			}
			return;
		}



        public void UpdateMousePosition()
        {
            Point point = window.MainPicBox.PointToClient(Control.MousePosition);
            if (window.MainPicBox.ClientRectangle.Contains(point))
            {
                PrintFlush(false);
                MoveMouse(point);
            }
        }

		public void LeaveMouse()
		{
			bool needRefresh = selectingButton != null || pointingString != null;
			selectingButton = null;
			pointingString = null;
			if(needRefresh)
			{
				RefreshStrings(true);
			}
		}

		public void MainPicBoxSizeChanged()
		{
			lastDrawnLineNo = -1;
			rgbValues = new int[window.MainPicBox.Width * (window.MainPicBox.Height * 2)];
		}

		private void barUpdate()
		{
			int max = displayLineList.Count;
			int move = max - window.ScrollBar.Maximum;
			if (move == 0)
				return;
			if (move > 0)
			{
				window.ScrollBar.Maximum = max;
				window.ScrollBar.Value += move;
			}
			else
			{
				if (max > window.ScrollBar.Value)
					window.ScrollBar.Value = max;
				window.ScrollBar.Maximum = max;
			}
			window.ScrollBar.Enabled = max > 0;
		}
		#endregion


		private bool OutputEmueraLog(string filepath)
		{
			StreamWriter writer = null;
			try
			{
				writer = new StreamWriter(filepath, false, Encoding.Unicode);
				foreach (ConsoleDisplayLine line in displayLineList)
				{
					writer.WriteLine(line.ToString());
				}
			}
			catch (Exception)
			{
				MessageBox.Show("ログの出力に失敗しました", "ログ出力失敗");
				return false;
			}
			finally
			{
				if (writer != null)
					writer.Close();
			}
			return true;
		}

		public bool outputLog(string filename)
		{
			if (filename == null)
				filename = "emuera.log";

			if (OutputEmueraLog(Program.ExeDir + filename))
			{
				if (window.Created)
				{
					bool notRedraw = false;
					if (redraw == ConsoleRedraw.None)
					{
						notRedraw = true;
						redraw = ConsoleRedraw.Normal;
					}

					PrintLine("※※※ログファイルを" + filename + "に出力しました※※※");
					if (notRedraw)
						redraw = ConsoleRedraw.None;
				}
				return true;
			}
			else
				return false;

		}

		public void GetDisplayStrings(StringBuilder builder)
		{
			if (displayLineList.Count == 0)
				return;
			for (int i = 0; i < displayLineList.Count; i++)
			{
				builder.AppendLine(displayLineList[i].ToString());
			}
		}

		public ConsoleDisplayLine[] GetDisplayLines(Int64 lineNo)
		{
			if (lineNo < 0 || lineNo > displayLineList.Count)
				return null;
			int count = 0;
			List<ConsoleDisplayLine> list = new List<ConsoleDisplayLine>();
			for (int i = displayLineList.Count - 1; i >= 0; i--)
			{
				if (count == lineNo)
					list.Insert(0, displayLineList[i]);
				if (displayLineList[i].IsLogicalLine)
					count++;
				if (count > lineNo)
					break;
			}
			if (list.Count == 0)
				return null;
			ConsoleDisplayLine[] ret = new ConsoleDisplayLine[list.Count];
			list.CopyTo(ret);
			return ret;
		}

		public void GotoTitle()
		{
			//if (state == ConsoleState.Error)
			//{
			//    MessageBox.Show("エラー発生時はこの機能は使えません");
			//}
            if (timer != null && timer.Enabled)
                stopTimer();
			displayLineList.Clear();
			ClearDisplay();
            logicalLineCount = 0;
            lineNo = 0;
            lastDrawnLineNo = -1;
            if (redraw == ConsoleRedraw.None)
                redraw = ConsoleRedraw.Normal;
            useUserStyle = false;
            userStyle = new StringStyle(Config.ForeColor, FontStyle.Regular, null);
            emuera.BeginTitle();
			state = ConsoleState.WaitKey;
			callEmueraProgram("");
			RefreshStrings(true);
		}


		bool force_temporary = false;
        bool timer_suspended = false;
		ConsoleState prevState;

		public void ReloadErb()
		{
			if (state == ConsoleState.Error)
			{
				MessageBox.Show("エラー発生時はこの機能は使えません");
				return;
			}
			if (state == ConsoleState.Initializing)
			{
				MessageBox.Show("初期化中はこの機能は使えません");
				return;
			}
            bool notRedraw = false;
            if (redraw == ConsoleRedraw.None)
            {
                notRedraw = true;
                redraw = ConsoleRedraw.Normal;
            }
            if (timer != null && timer.Enabled)
            {
                timer.Stop();
                timer_suspended = true;
            }
            prevState = state;
			state = ConsoleState.Initializing;
			PrintSingleLine("ERB再読み込み中……", false, true);
			force_temporary = true;
			emuera.ReloadErb();
			force_temporary = false;
            PrintSingleLine("再読み込み完了", false, true);
			RefreshStrings(true);
            //強制的にボタン世代が切り替わるのを防ぐ
            updatedGeneration = true;
            if (notRedraw)
                redraw = ConsoleRedraw.None;
        }
		public void ReloadErbFinished()
		{
			state = prevState;
			PrintLine(" ");
            if (timer_suspended)
            {
                timer_suspended = false;
                timer.Start();
            }
		}
		public void ReloadPartialErb(List<string> path)
		{
			if (state == ConsoleState.Error)
			{
				MessageBox.Show("エラー発生時はこの機能は使えません");
				return;
			}
			if (state == ConsoleState.Initializing)
			{
				MessageBox.Show("初期化中はこの機能は使えません");
				return;
			}
            bool notRedraw = false;
            if (redraw == ConsoleRedraw.None)
            {
                notRedraw = true;
                redraw = ConsoleRedraw.Normal;
            }
            if (timer != null && timer.Enabled)
            {
                timer.Stop();
                timer_suspended = true;
            }
            prevState = state;
			state = ConsoleState.Initializing;
            PrintSingleLine("ERB再読み込み中……", false, true);
			force_temporary = true;
			emuera.ReloadPartialErb(path);
			force_temporary = false;
            PrintSingleLine("再読み込み完了", false, true);
			RefreshStrings(true);
            //強制的にボタン世代が切り替わるのを防ぐ
            updatedGeneration = true;
            if (notRedraw)
                redraw = ConsoleRedraw.None;
        }

		public void ReloadFolder(string erbPath)
		{
            if (state == ConsoleState.Error)
			{
				MessageBox.Show("エラー発生時はこの機能は使えません");
				return;
			}
			if (state == ConsoleState.Initializing)
			{
				MessageBox.Show("初期化中はこの機能は使えません");
				return;
			}
            if (timer != null && timer.Enabled)
            {
                timer.Stop();
                timer_suspended = true;
            }
            List<string> paths = new List<string>();
			SearchOption op = SearchOption.AllDirectories;
			if (!Config.SearchSubdirectory)
				op = SearchOption.TopDirectoryOnly;
			string[] fnames = Directory.GetFiles(erbPath, "*.ERB", op);
			for (int i = 0; i < fnames.Length; i++)
				if (Path.GetExtension(fnames[i]).ToUpper() == ".ERB")
					paths.Add(fnames[i]);
            bool notRedraw = false;
            if (redraw == ConsoleRedraw.None)
            {
                notRedraw = true;
                redraw = ConsoleRedraw.Normal;
            }
            prevState = state;
			state = ConsoleState.Initializing;
            PrintSingleLine("ERB再読み込み中……", false, true);
			force_temporary = true;
            emuera.ReloadPartialErb(paths);
			force_temporary = false;
            PrintSingleLine("再読み込み完了", false, true);
			RefreshStrings(true);
            //強制的にボタン世代が切り替わるのを防ぐ
            updatedGeneration = true;
            if (notRedraw)
                redraw = ConsoleRedraw.None;
        }
	}
}