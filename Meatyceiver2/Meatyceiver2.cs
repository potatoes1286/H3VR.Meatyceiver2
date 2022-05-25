using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

namespace Meatyceiver2
{
	[BepInPlugin("dll.potatoes.meatyceiver2", "Meatyceiver2", "0.5.0")]
	public class Meatyceiver : BaseUnityPlugin
	{
		public const string PLUGINS_DIR_PATH = "BepInEx/Plugins";

		private readonly Script				_lua;
		private readonly DirectoryInfo[]	_pluginDirs;
		private readonly List<ConfigScript>	_scripts;

		public Meatyceiver()
		{
			_pluginDirs = new DirectoryInfo(PLUGINS_DIR_PATH).GetDirectories();

			_lua = new Script()
			{
				Options =
				{
					ScriptLoader = new FileSystemScriptLoader()
					{
						ModulePaths = new []{ PLUGINS_DIR_PATH }
					},
					ColonOperatorClrCallbackBehaviour = ColonOperatorBehaviour.TreatAsColon
				}
			};
			
		}

		private void Awake()
		{
			StartCoroutine(LoadScriptDirectory());
		}

		private IEnumerator LoadScriptDirectory()
		{
			return _pluginDirs.Select(dir => StartCoroutine(LoadScripts(dir))).GetEnumerator();
		}

		private IEnumerator LoadScripts(DirectoryInfo dir)
		{
			yield return (from file in		dir.GetFiles() 
									where	file.Extension == "lua" 
									select	StartCoroutine(LoadScript(file))).GetEnumerator();
		}

		private IEnumerator LoadScript(FileInfo file)
		{
			var script = String.Empty;
			using (var reader = new StreamReader(file.OpenRead()))
			{
				while (!reader.EndOfStream)
					yield return script += reader.ReadLine();
			}
			_scripts.Add(new ConfigScript(_lua, script));
		}
	}
}
