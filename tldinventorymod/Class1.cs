using Il2Cpp;
using MelonLoader;
using MelonLoader.Utils;
using System.IO;
using UnityEngine;

[assembly: MelonInfo(typeof(PPPInfoMod.Main), "PPP Mod", "1.0.0", "Blades")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace PPPInfoMod
{
	public class Main : MelonMod
	{
		int m_doses = 0;
		int m_daysSurvived = 0;

		float m_zeroDaysStartTime = -1f;
		float m_zeroDosesStartTime = -1f;
		float m_mainMenuCheckStartTime = -1f;

		const float ZERO_DELAY = 10f;   // Delay before accepting a zero reading
		const float MENU_DELAY = 5f;    // Delay before resetting stats when at main menu

		private string daysFilePath;
		private string dosesFilePath;

		public override void OnApplicationStart()
		{
			MelonLogger.Msg("PPP Mod Loaded Successfully");

			// File paths (creates in game directory)
			daysFilePath = Path.Combine(MelonEnvironment.GameRootDirectory, "days.txt");
			dosesFilePath = Path.Combine(MelonEnvironment.GameRootDirectory, "doses.txt");

			// Create files if missing
			if (!File.Exists(daysFilePath))
				File.WriteAllText(daysFilePath, "0");
			if (!File.Exists(dosesFilePath))
				File.WriteAllText(dosesFilePath, "0");
		}

		public override void OnUpdate()
		{
			//Detect if game is really unloaded (main menu)
			bool noWorld = GameManager.GetPlayerManagerComponent() == null;

			if (noWorld)
			{
				// Start reset timer if no world
				if (m_mainMenuCheckStartTime < 0f)
					m_mainMenuCheckStartTime = Time.time;

				// Only reset after 5 seconds of confirmed no world
				if ((Time.time - m_mainMenuCheckStartTime) >= MENU_DELAY)
				{
					if (m_daysSurvived != -1 || m_doses != 0)
					{
						m_daysSurvived = -1;
						m_doses = 0;
						m_zeroDaysStartTime = -1f;
						m_zeroDosesStartTime = -1f;

						WriteToFile(daysFilePath, "0");
						WriteToFile(dosesFilePath, "0");
					}
				}
				return;
			}
			else
			{
				m_mainMenuCheckStartTime = -1f;
			}

			var inv = GameManager.GetInventoryComponent();
			var ach = GameManager.GetAchievementManagerComponent();
			if (inv == null || ach == null)
				return;

			// ---- DOSES CHECK ----
			int pillDoses = inv.GetNumGearWithName("GEAR_BottlePainKillers") / 6;
			int stimDoses = inv.GetNumGearWithName("GEAR_EmergencyStim");
			int currentDoses = pillDoses + stimDoses;

			if (currentDoses == 0)
			{
				if (m_zeroDosesStartTime < 0f)
					m_zeroDosesStartTime = Time.time;

				if ((Time.time - m_zeroDosesStartTime) >= ZERO_DELAY && m_doses != 0)
				{
					m_doses = 0;
					WriteToFile(dosesFilePath, "0");
				}
			}
			else
			{
				m_zeroDosesStartTime = -1f;
				if (m_doses != currentDoses)
				{
					m_doses = currentDoses;
					WriteToFile(dosesFilePath, m_doses.ToString());
				}
			}

			// ---- DAYS SURVIVED CHECK ----
			int currentDaysSurvived = ach.m_NumDaysSurvived;

			if (currentDaysSurvived == 0)
			{
				if (m_zeroDaysStartTime < 0f)
					m_zeroDaysStartTime = Time.time;

				if ((Time.time - m_zeroDaysStartTime) >= ZERO_DELAY && m_daysSurvived != 0)
				{
					m_daysSurvived = 0;
					WriteToFile(daysFilePath, "0");
				}
			}
			else
			{
				m_zeroDaysStartTime = -1f;
				if (m_daysSurvived != currentDaysSurvived)
				{
					m_daysSurvived = currentDaysSurvived;
					WriteToFile(daysFilePath, m_daysSurvived.ToString());
				}
			}
		}

		private void WriteToFile(string path, string text)
		{
			try
			{
				File.WriteAllText(path, text);
			}
			catch
			{
				MelonLogger.Error("Failed to write to file: " + path);
			}
		}
	}
}
