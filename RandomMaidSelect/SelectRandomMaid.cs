using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using HarmonyLib;

namespace COM3D2.SelectRandomMaid.Plugin
{
    [BepInPlugin("org.bepinex.plugins.COM3D2.SelectRandomMaid.Plugin", "Select Random Maid", "1.0.0.0")]
    public class RandomMaidSelect : BaseUnityPlugin
    {
        private void Awake()
        {
            randomSelectKeyConfig = Config.Bind("Shortcuts", "Key", KeyCode.R, "Select Random maid shortcut Key");
            randomSelectVRKeyConfig = Config.Bind("Shortcuts", "VR Key", AVRControllerButtons.BTN.VIRTUAL_MENU, "Select Random maid VR shortcut");
            randomButton.LoadImage(imageData);
        }

        private void OnLevelWasLoaded(int Level)
        {
            CurrentLevel = Level;
        }

        private void Update()
        {
            if (CurrentLevel == 1 || CurrentLevel == 36 || CurrentLevel == 21) 
            {
                if (Input.GetKeyDown(randomSelectKeyConfig.Value))
                {
                    SelectRandomMaid();
                }

                if (GameMain.Instance.VRMode)
                {
                    AVRControllerButtons isVRControllerLeft = GameMain.Instance.OvrMgr.GetVRControllerButtons(true);
                    AVRControllerButtons isVRControllerRight = GameMain.Instance.OvrMgr.GetVRControllerButtons(false);
                    if (isVRControllerLeft.GetPressDown(randomSelectVRKeyConfig.Value) || isVRControllerRight.GetPressDown(randomSelectVRKeyConfig.Value))
                    {
                        SelectRandomMaid();
                    }
                }
            }
        }

        private void SelectRandomMaid()
        {
            // Yotogi & Karaoke
            if (CurrentLevel == 1 || CurrentLevel == 36)
            {
                GameObject maidSkillUnit = GameObject.Find("UI Root/Parent/CharacterSelectPanel/Contents/MaidSkillUnitParent");
                UIWFTabButton[] buttonsList = maidSkillUnit.GetComponentsInChildren<UIWFTabButton>();

                int randomButtonIndex = UnityEngine.Random.Range(0, buttonsList.Length);
                Logger.LogInfo("Number selected:" + randomButtonIndex);

                AccessTools.Method(typeof(UIWFTabButton), "OnClick").Invoke(buttonsList[randomButtonIndex], null);
            }
            // Dance
            else if (CurrentLevel == 21)
            {
                bool isSingleMaidDance = true;
                GameObject maidSkillUnit = GameObject.Find("UI Root/Parent/CharaSelect/CharacterSelectPanel/Contents/MaidSkillUnitParent");
                UIWFTabButton[] buttonsList = maidSkillUnit.GetComponentsInChildren<UIWFTabButton>();

                // If multiple maids to select the type of button changes.
                if (buttonsList.Length == 0)
                {
                    isSingleMaidDance = false;
                }

                if (isSingleMaidDance)
                {
                    Logger.LogInfo("Number of maids found:" + buttonsList.Length);
                    int randomButton = UnityEngine.Random.Range(0, buttonsList.Length);
                    Logger.LogInfo("Maid number " + +randomButton + " selected");

                    AccessTools.Method(typeof(UIWFTabButton), "OnClick").Invoke(buttonsList[randomButton], null);
                }

                if (!isSingleMaidDance)
                {
                    Logger.LogInfo("Multiple Maids Dance");
                    UIWFSelectButton[] SelectList = maidSkillUnit.GetComponentsInChildren<UIWFSelectButton>();
                    Logger.LogInfo("Number of maids found:" + SelectList.Length);

                    // Unselect already selected Maids
                    // Determines if the list has, at least, one selected maid first, to avoid conflict if the user cancels or after another dance.
                    foreach (UIWFSelectButton buttonInList in SelectList)
                    {
                        if (buttonInList.isSelected)
                        {
                            oneMaidIsSelected = true;
                            break;
                        }
                    }

                    if (randomMaidList.Count != 0)
                    {
                        if (oneMaidIsSelected)
                        {
                            foreach (int value in randomMaidList)
                            {
                                Logger.LogInfo("Unselecting Maid: " + value);
                                UIWFSelectButton.selected = false;
                                AccessTools.Method(typeof(UIWFSelectButton), "OnClick").Invoke(SelectList[value], null);
                            }
                            oneMaidIsSelected = false;
                        }
                        randomMaidList.Clear();
                    }

                    // Check if enough maids to avoid having an infinite while loop
                    if (SelectList.Length < 3)
                    {
                        Logger.LogInfo("Not enough Maids");
                        return;
                    }

                    // Select 3 maids 
                    while (randomMaidList.Count < 3)
                    {        
                        int randomButton = UnityEngine.Random.Range(0, SelectList.Length);
                        if (!randomMaidList.Contains(randomButton))
                        {
                            randomMaidList.Add(randomButton);
                            Logger.LogInfo("Selecting Maid: " + randomButton);
                        }
                    }

                    foreach (int value in randomMaidList)
                    {
                        AccessTools.Method(typeof(UIWFSelectButton), "OnClick").Invoke(SelectList[value], null);
                    }
                }
            }
        }

        private void OnGUI()
        {
            if (CurrentLevel == 1 || CurrentLevel == 21)
            {
                    if (GUI.Button(new Rect(Screen.width / 2 + 60, Screen.height - 128, 128, 128), randomButton))
                    {
                        SelectRandomMaid();
                    }
            }
            if (CurrentLevel == 36)
            {
                GameObject maidSkillUnitKaraoke = GameObject.Find("UI Root/Parent/CharacterSelectPanel/Contents/MaidSkillUnitParent");
                if (maidSkillUnitKaraoke != null)
                {
                    if (maidSkillUnitKaraoke.gameObject.activeInHierarchy)
                    {
                        if (GUI.Button(new Rect(Screen.width / 2 + 60, Screen.height - 128, 128, 128), randomButton))
                        {
                            SelectRandomMaid();
                        }
                    }
                }
            }
        }

        private int CurrentLevel;

        private bool oneMaidIsSelected = false;

        private List<int> randomMaidList = new List<int>();

        private ConfigEntry<KeyCode> randomSelectKeyConfig;

        private ConfigEntry<AVRControllerButtons.BTN> randomSelectVRKeyConfig;

        public Texture2D randomButton = new Texture2D(128, 128, TextureFormat.RGBA32, false);

        byte[] imageData = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAIAAAACACAYAAADDPmHLAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAACxM" +
            "AAAsTAQCanBgAAAYVaVRYdFhNTDpjb20uYWRvYmUueG1wAAAAAAA8P3hwYWNrZXQgYmVnaW49Iu+7vyIgaWQ9Ilc1TTBNcENlaGlIenJlU3pOVGN6a2M5ZCI/Pg0KPHg6eG1wbWV0YS" +
            "B4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNS42LWMxNDUgNzkuMTYzNDk5LCAyMDE4LzA4LzEzLTE2OjQwOjIyICAgICAgICAiPg0KICA8c" +
            "mRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPg0KICAgIDxyZGY6RGVzY3JpcHRpb24gcmRmOmFib3V0PSIiIHhtbG5z" +
            "OnhtcD0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLyIgeG1sbnM6ZGM9Imh0dHA6Ly9wdXJsLm9yZy9kYy9lbGVtZW50cy8xLjEvIiB4bWxuczpwaG90b3Nob3A9Imh0dHA6Ly9" +
            "ucy5hZG9iZS5jb20vcGhvdG9zaG9wLzEuMC8iIHhtbG5zOnhtcE1NPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvbW0vIiB4bWxuczpzdEV2dD0iaHR0cDovL25zLmFkb2JlLm" +
            "NvbS94YXAvMS4wL3NUeXBlL1Jlc291cmNlRXZlbnQjIiB4bXA6Q3JlYXRvclRvb2w9IkFkb2JlIFBob3Rvc2hvcCBDQyAyMDE5IChXaW5kb3dzKSIgeG1wOkNyZWF0ZURhdGU9IjIwM" +
            "TktMTItMjlUMjM6MDk6NTItMDg6MDAiIHhtcDpNb2RpZnlEYXRlPSIyMDE5LTEyLTI5VDIzOjIxOjExLTA4OjAwIiB4bXA6TWV0YWRhdGFEYXRlPSIyMDE5LTEyLTI5VDIzOjIxOjEx" +
            "LTA4OjAwIiBkYzpmb3JtYXQ9ImltYWdlL3BuZyIgcGhvdG9zaG9wOkNvbG9yTW9kZT0iMyIgcGhvdG9zaG9wOklDQ1Byb2ZpbGU9InNSR0IgSUVDNjE5NjYtMi4xIiB4bXBNTTpJbnN" +
            "0YW5jZUlEPSJ4bXAuaWlkOjAzMzNiNTcwLWM3MjktYjI0Ny04NjJhLWE3ZjZjZDcxYmZiOSIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDowMzMzYjU3MC1jNzI5LWIyNDctODYyYS" +
            "1hN2Y2Y2Q3MWJmYjkiIHhtcE1NOk9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDowMzMzYjU3MC1jNzI5LWIyNDctODYyYS1hN2Y2Y2Q3MWJmYjkiPg0KICAgICAgPHBob3Rvc2hvc" +
            "DpUZXh0TGF5ZXJzPg0KICAgICAgICA8cmRmOkJhZz4NCiAgICAgICAgICA8cmRmOmxpIHBob3Rvc2hvcDpMYXllck5hbWU9Ik1haWQgRXhhbSIgcGhvdG9zaG9wOkxheWVyVGV4dD0i" +
            "TWFpZCBFeGFtIiAvPg0KICAgICAgICA8L3JkZjpCYWc+DQogICAgICA8L3Bob3Rvc2hvcDpUZXh0TGF5ZXJzPg0KICAgICAgPHhtcE1NOkhpc3Rvcnk+DQogICAgICAgIDxyZGY6U2V" +
            "xPg0KICAgICAgICAgIDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJjcmVhdGVkIiBzdEV2dDppbnN0YW5jZUlEPSJ4bXAuaWlkOjAzMzNiNTcwLWM3MjktYjI0Ny04NjJhLWE3ZjZjZDcxYm" +
            "ZiOSIgc3RFdnQ6d2hlbj0iMjAxOS0xMi0yOVQyMzowOTo1Mi0wODowMCIgc3RFdnQ6c29mdHdhcmVBZ2VudD0iQWRvYmUgUGhvdG9zaG9wIENDIDIwMTkgKFdpbmRvd3MpIiAvPg0KI" +
            "CAgICAgICA8L3JkZjpTZXE+DQogICAgICA8L3htcE1NOkhpc3Rvcnk+DQogICAgPC9yZGY6RGVzY3JpcHRpb24+DQogIDwvcmRmOlJERj4NCjwveDp4bXBtZXRhPg0KPD94cGFja2V0" +
            "IGVuZD0iciI/PvZZndsAACGBSURBVHhe7V17cFXVud95v0hySEhCgBAC8hAEgfAQaJVSwNZrp4CPq3Nbp9I/qGNvZzq2tY9BbW2tpTpq66u0o4hSvOAddTqDF0sxAyjIo5R3gYSIkCc" +
            "JhDzIO7nfb7F/x+/snBNOyDkh0f2b+bLWXvtxTvbv933rW2s/ToTVP4Dv4fwukWJsp6GNppdRj1LLbIMBbCP0OqLDLoFO29iGEsY2f8Z1ehuAbTANrr+mcJ6EvkYgUjSJuoy2S5qTdK" +
            "zncbDMkkag3R9Ikj8y9XK7qrfZy871KGn6eADqBNrY3ucIdCLCDScZ/B6aLJLrj2QY2vU2JN+5PQ3gZ/HznNAkadJIqCYadZKv61jPbbms23lcgCWAdlifItCJCAecJ57LNJDkJJR1G" +
            "smn6XXaSDqMx3Z+nhM8+SQCRsJALkpNKA3rSL5TEDDn9vp4FADqMEILI6zwdyLCAX3SWZIUTRgJdJKsl2NUnet0CeOxNPG6DvMHEkFyYKiDOE0k6iS71S5JOI3ter3el8fjZ6EE+Plc" +
            "DisCnYhQgcfXJYlASaK0kWCUMLSR8Fi7zdnO/aKWLVs2YsyYMYM9Hk9iUlJS/KBBg2IiIyOjEhISojo7OyNgsp1fREREdMLa29s7W1tb28U6ampqGuvq6hrPnj17cceOHeUnT56sk01" +
            "JIsgl4c7lFj9tMIpCC4EGoNQCCKsQAp6MEAEEA/gcGomHkTxdapJpaEfJddFCblx2dnZSVlZWUmpqarwgThC1ZMmSGaNGjcqVNo+QnyzbJUYJkpOTcYyg0NLS0tHc3Nze1tbWflFQW1" +
            "t7saSkpHTXrl2Hjx07Von2hoaG5urq6oZTp05hfbOIhkSjpEEErHOdLmEQBCMDyKYYtADCJoJwCoDko9TEow4ynKaJjrNLTTrKuBiBeHTslClT0u64445JM2bMmDRSkJmZOSQlJSVGP" +
            "Fg2Cx9EGB0SEZrPnTtXdeTIkX+vXbt2+86dO8vr6+ubRDitHR0dJL5ZlU5hcBunCCgELQJtIUeozxaOp4+pSYeH00i0NhIN8g3Zqh4jHh47ffr0zJ/97Gf/MWnSpOuHDh3qEY/HcYRz" +
            "A6n2DaQbsYtO80fEUF9cXPyJdBH71q1bd/DAgQNVsh4kg3wKgdGAdbRrQVAAFAUFoKMBgOWQIZRnTZMP0llq0jX5INZpIDzeLg35y5cvH3/bbbdNF4+fNGLEiHSJ5rEI6RIFIvuS9O6" +
            "AnEEgRXtbk0DyhfNbt27duXLlygLpQRpkE006rMku2Q4BMEpQADBGBAqA5IdMBKE6gySfpkN9IPIT7FJ7Okq0x7/88suLZ8+efYP08xnSjydL5E9AHy/r+jWkC+iUrqBdrL6qqurCoU" +
            "OHTqxevbpg06ZNZ2S1FoAWAdt1RKAAdLfAiACERAQgqbfQxMOc5INwEk/yvUTbZkSQlpaW9Mtf/vKWWbNmjcvJycmTRC5NSIfH43gDDhhFIIGsrKw8W1RUdFpyhZNPPPHEPlkF8hvtk" +
            "kKgGCgIHRFAtlMIjAq6e+gxentiSToN5NM08SBdG8inxc+ZM2fo3LlzR06bNm2UeP0sIT8nNjY2qr+E+N5CRg2dMpy8KF3D6b179x7ctm1b4caNG09Ib3FJVkMIFADFgFJ3Dbo70N1C" +
            "r0XQ25AKolnCcDxnqIfBw+ntID1JLFGy9hQhfvjSpUunfO1rX5sn9S8PGzZsSHR0dL/p30MBpCuJiYnx6enpmaNHjx7r8XiiJDrUSX7QKiJADqGdCEAJUp0nge0hQ28EEAz5mnSaIV/" +
            "69RQZwg1/5JFHlt16660Lxo4dO0oyfez7uQW6MhFC7Lhx40ZLUjteuofysrKyxoaGhk7JHXgeAYqBdiVctTCCObgT+ktp8kGeNpKvBcB60uuvv37HN77xjfmDBAO1j+8NMITEhNKePX" +
            "v2y9Bx64svvnhImtEdYNSArgHmzA/QFbB05gQA6j1CTyNAd+TrkE/CE8UGicHrkyQEeubPn5+3YcOG/5YyPwHzs19A8gF0cRjNyignMz8/f/yCBQuGFRYWVpSUlIBgnBOcW55vnmvC2" +
            "T2gzrYeRYKeCIBfBtYd+fR6km8EIP/ksAceeGDWww8//F+jRo0agewefaOs+0ID8xniB/HDhw/PWrRo0UQZOlYcP368TroEnmeec8IfwZp8lkEhWAHwS9C6I7+L999+++1jfvCDH3xZ" +
            "+vqbhw4dOgwzeJ+nJK+3gAhiYmJikwXiKMMlKayVaFDb2NiIkM5zTgQiV7cHfXKDEQC/AI3kw/x5Pr0flvTggw9O/fa3vz1XEr7pEu5GSIYf9Jf7IgEOIecmKjU1NV1GQglZWVkRlZW" +
            "VdWLsEnjeQDTtSrjiuQ5GACCcJc3p/fR8lob8hQsX5q5YseLWWbNmzRgicL3+ykC3KI6SLUPFVEkSa8rLy+urq6sxHwCQeCZ7FEGwguiCYCMAiUddkw9zEg9LSEtL86xateqeL33pSz" +
            "OQ6Uubix4gMzNTcub0weI09fv3769qaWkBwTj/OuvXxOt2ehqXA+JKAgDpAEWgideeT+KN54P8tWvXLl+8ePFNuE4vbS6uAtINDBk5cmRWXl5e3KZNmz6VJk24P3C93g7cBdwnkABIO" +
            "EDvB+nYXpMPA+nebB9hH54P8iWxgWBcXCXQZUpOkJKbmzt05syZQ956663jaL681odkQC87CQ8ogu4EQBHAdNgH+ebijRi830s+Ej70+Qj7rueHBsgJ5FwmIBJMmzYtZfPmzYW4Y8le" +
            "rUnVXQDgJDxoAZB8LQBn6Cf5JuTDMNRDto+Ez+3zQwsME2MFMjrIqKurK5MhYo0METEbCFK1UQQwDeeyF04BkHiAng8D8RCB0/sN+ZjkwTgfQz1k+9LmIsSQ7gDXETA8TKyoqCiT0UH" +
            "DpUuXMB1MwmGcHgZ0lAhaACAdoPdjvfZ+EM++3wgA07uY4cMkD8b57lAvvMjIyMiSc9xQWlpahQtJuJ4gzZp8GuCPeJ+2QBGAoR/GsK8TP3r/IMztY3oXM3zuJE/4gZwgJydnjHh/aV" +
            "FR0flz587hQhFIBenOC0TaAJZeaAH4837t+c5+H2UKLuzYc/tuxt9HkHMdKQ6XIw5Xs2XLllJpIskQACOC0ygKHyfVAtDej3aY9nwQ7jVcz3/llVfulggwHRd23NDft0hOTk70eDzxI" +
            "oSOgoKCcmmi5wM6CgC6ztKAAqD3o9Tez6SPYd94P+7kwc0cjz766H24pIuwJO0u+hA456mXEbFnz55iSQpx7wBGBiBbRwEtBpZevigA7f0I5TQKwCf033zzzSMef/zx/xw9evSoL+r1" +
            "/P4AOffREn3jrrvuukFvvvnmUWkC8fpmEU06jTB1LQB6P8lnBNDenzhnzhzcw3fj4sWLF4jzYxsX1wjodkUEsdIVJNfW1pYcOnSouqOjAwJgJCD5WhDaDOH0foAigDB0N0CLw927N91" +
            "001QJPRCFi2sMCQAxQ4YMwWNyczFjKE3kCtzB4MzgktySa8O7FoAmHqYjgRkF4L593Lo9YcKESbLsop9ARgOxM2fOnJ2bm5sqEQERG3xRBOSRQiDXhnc2wEi6l3DbzHgftmrVqoWS9c" +
            "/Drduy7KKfAAlhYmJitOQCg3fv3n28qqoKcwMI+Qz9MGe34O0CWDICUC0Uggn9YvH2Ezs5UnfRDzFv3rz89PT0ZKnSeQN1B3R6s0DDSu7AAyCcmAjw8ssv3yYCmCZ9f4o75u+fiImJi" +
            "ZLoHFtWVlZSXFyMF1kgAtCco4MuSSCjAL2fZpSEBzWF/DSX/P4LcDN16tQb8/LyBssinVhzCUcnz4Z788de0NEAhh1MRMAj2tnZ2RmY8ZNlF/0YkqinylB9rDhshiySQ4Z+Gnn3OwrA" +
            "xjSjHDyfj0e03Umf/g9x0sjJkydfP2PGjGxZBPna+wMKAKVTAEY9MrbE61gm4fl8WXYxADBq1Kgx48ePz8SzBrLICAA+UZJrwz0rXEHyuWMcXsuCN3OIsiAOFwMAmZmZuI8wY9y4cR5" +
            "Z1CKA+UQC/GGigI047WtMFJT4/PPP34sndyX8uwIYWIgZNGhQ8/vvv39a6oHeOtLJCKC9H2bCBW5FwwuZXPIHHiQKDJ0wYcIIqTL0Mwp4vR+GP/4SwChJ+uLwKja8jUuEgPUuBhDE+x" +
            "Nl5DZ44sSJ6AYgAh9+xQz3RgV2g7Zo2TnpzjvvvEH6fvdBzgEI6b4jPR5P2tKlS8fLIklnCTPcUwBaCEYleANnfn7+RKm77A9QSBT3zJw5c5xUted7yYfhj+4CvJaamhqPN3CK97sCG" +
            "KCQIfygnJyc4VIl+eSZAjBdABu4EUq8mTMOr191+R+4sDlMl6rh1C6Ng4sZ7qkErADTZqWM+xNl6JeGd+/KsosBitjY2EjhkKRrZ/eKAH9SxXzmABYvXjxSLF+GgOg/eoQ9e/ZYpaWl" +
            "fq2urs5csEhMxN1lAwcffvihdebMGUt6RLtl4KC9vb0jPT29cdu2bUVtbW24T4BzAjDzHhpc3wcjKbQf//jHs2QEgMu/U2S5R7j33nvtWmDccMMN1ne/+11Lhph2S/9Fa2urdd9995n" +
            "6n/70J0s8ytQHCpqampp37Nixf8mSJc83NDRUS1OtGC4V4y1kLQgHTAJpUTJ8SJQkEOPHq0ZeXp513XXXWddff701efJka/r06dbo0aPNusOHD1tPPPGE8SoX4YVE3Cg8rxkZaaK95h" +
            "nW5WqgMRk+4Jc2cGfJVeOBBx6wHn/8ceuRRx6xfv7zn1sSVazf/OY31htvvIFr1ta5c+fMsovwAldwxZlT7ck8bYZ3VACfRiE/RkQQlo4as8o/+tGPMFNlXbx40dq1a5e9xkU4IBEgE" +
            "m8fkyr59fIs5iVeWwTCRTjn/3HoMWPGmHplZaUpXYQHSLpxq5hAk+/l21sRA8xKvMBTRIPhQ9ggQ01T1tYiJ/GPAwcOWP/4xz+s7du3WyUlJXarf/zzn/80hpcvAzjuxx9/bG3atMm0" +
            "ByM0fN57771nbd682Tp+HG9jCQ547euhQ4esLVu2WAUFBdaRI0fsNf7B70pg+7///e9mtOHve2JbfC9Ey/Pnz9utwUGcOQLDefmO5Jneb5Y1yVzR7S9rhQoyJDHl5a7JF3v37rXefPP" +
            "NLqTHx8db999/v3XzzTfbLZ/hhRdesC5dumQtX77cDDc3btxor7kM8QKTl8yZM8du8cXzzz9vCNBArvL973/fXvIPkPLiiy+a0YIGhro/+clPrPHjMRX/GYqKiqzf//73po79fve731" +
            "mnT+OK7WdYunSpdffdd5uhM/6vU6dO2Wsug+uDhT2Z5+XXNgN99tFoVNEXAsCJADIycOvaZ4DnPf3004Z8jCDuuusuQ+qiRYuMd7/00kvGWwIB3gLyMepANzNz5kzTDoL+8Ic/WOXle" +
            "JDWFxAbyV+wYIH1rW99y7r11lutf/3rX90mqiD/ueeeM8eWqGl95Stf8YoTYgTRxcXFZtkfnnnmGUM+9pk/f76Vm5tr2t9++20T9Z599llDvr/1H330kan3AOTaRwCcAfRpDDeOHTtm" +
            "FRYWmvq4cb5zTa+99popb7nlFut73/ueqRPZ2dnW2rVrrQ0bNhhB+APC6apVqyz9+MLZs2etX/3qVyYyvPvuu9aKFSvsNZYJqWgD8Hn4XOLOO++0Hn30UXupK/7yl7+YEpHi4YcfNnU" +
            "AkQbCgEAw6lm5cqW9xhf4f/C9iPb2duu3v/2t+R9effVVQ/iaNWtwn5+9xeVIJ+N6I5C5c+farUFD82zqUEVYiO/o6DD/kDZ4HzztqaeeMttMmTLFq2oC8wM4mU7yga9//etmIqa+vr" +
            "5L2CS++c1v+pAPIN9A2AScHglxABCiJh/ASAUi8IeTJ09aDQ0Nlsfj8SGfWLZsmSmPHj1q1dTUmLoTiBgaSI4XL15s6o2NjeY7a/KBQP9HkHBy7R0Ghhw//elPTSjV9sMf/tD0tQiPm" +
            "CjCbKA/wKMCAZNLQEVFhSmdyMrKsmu+GD4cF8Usq6oKv+j2GTgZFWg/Zx9OYB4DgBf7A0SIvAOAYJ2Ijo62JkyYYC99BibHgL+HsIYNG2a6Gwyhnf/L1SBsAugOCF3w9MzMTLslMOAJ" +
            "EAyNySMijD8gUfSHy0Nhy3gtsnYC3QKQlIQHoLqC+zmB7wJ0d12Dx3QmiMCVvicQ6LN53J6OCPwhbAJAP7x+/XofY7994cIFUwYCcoTHHnvMdANIABEpaAcPHjTboEvxh0DC6G8I9P1" +
            "7glAcAwLQZ+wz1wgDvvrVr5oSBGOI4w/IbpEYYTSA/hWjhHnz5lmzZs0yXQPCHxDonw/FSekL9EaoV4qC3QA7kGOU5oeKALNwuWqZX9C26yEFEj7OAGIo40Rzc7PJ8gEMw5588kkzdM" +
            "NYHPkDki32kYH++S9CBLjKfTXxXn51F8AVYRMAwCiAoYzuiwGM/ZHcQCjf+c537FZfxMbi1oWBj94ItRcCoHkBAehGU5cPwM+fhsWVMPSxZ6bMeF6DkzSBMmsg1AIYPBgP0gaekma27" +
            "wS6JyBQPgOCOfzzl/D1lQDgZPjFc1RtA1g388Oo6A06JGttx+/nSz0sYBR45513TEnwZgtEgUBANxFKsEvZtw+/6NoVga4JcNj46aef+v2+uOeBCPVNJD0Rj2zbiRdLi9Npjr2cMwlk" +
            "gzERQIckGmETwMKFC+2aZW3bts2uXR7jAkgS9cUSAjNrmFgJJSgACAtDUw3MyK1evdpe8gXG6NgXCdm6det8RIC5BcwEAkheAw0xrxZMAoMBIgAcWuDl1zbDOXMANhiT0NUo/1BgN+w" +
            "ldDKou4G0tDQzkwdgHv3Xv/61mW7985//bG41w0nl5EpPEchrUlNTvbd84Yoebl7BZBUmsvD5N954o1nnD9gGQsC0LBJVLD/00EPmIhDmCSZOnGg9+OCD9tahgzN36g7yf3dUCxAJZJ" +
            "FGvr3DQBg83jw8WFdX1yh9YtgEALAbkO9mplWJe+65x5CNmTJ4IC4Hb9261dw/iJN5tfcRBhIAgCnmJUuWmKlYTLFiuhpTzZgaxugjENLT083IJD8/33gl9uHwFpNdWNfTnKUn5AYDO" +
            "V77+fPnkah4+RUj5+amUMyRIkZ5bwq9//77J991110L5MTMl+VrBkx1wpAUwlMBzAwCnA8gArVrXGkbeC1n15Dk4VpAsMAMI67lQ7j4yYTefI/erteQbS9t3Lhxy4oVK9Y3NTVBCPqm" +
            "0Gbc9YP5RsRVGOQai39GsuMIydhnyfI1A6ZZcTJ1Fo0uwF83EKhd40rbYB0SNlhPPRfbY0QBoYbie/RmvYbkNg0rV658q7CwsFKiAZTD3yPG/HSb7gJo7RKS6yQRq7SHDy4GKDCcFw5" +
            "bNm/eXIYsEE1iPnxTAFiBzse7AUYBkgs0h7pPctF3sDnEL492IZ6mh4FUCBKFNukGms+dO4frja4CBijwUIjkJUhqwClCPjnuIgAYM0RYq2TnDZKF/9uNAAMX4sQXpSvHEEuT7zMSoA" +
            "DYSKW0nTp16uLatWu3iwBcBQxQXLx4sWLNmjW7pQpODa9iFIIRgc4BKABT1tbWNu/cubNcuoF6JBPS5mIAAflbeXl5ze7du3GfOXnVkQBmBABynVGgTUhvra+vbyouLv5EggDaXQwg1" +
            "NTUVH/yyScVly5dwpAPnJJ88gzOfa4FeMkXM+GipaWldceOHftEDGhzMYBQUlLy6d69e3HvPTmleclHqSMAzEcEHR0drevWrTso2WSTPZfsYgBAHLftzJkzn27atKm7dwQaEegcoIsA" +
            "xFoOHDhQdfbs2fNyUKxzMQBQVVVVe/r0aXQBmPbVAoBpvn2GgSTfEG+XZspw69atO0UAXe9tdtEvcfjw4YP79u3D/e7gkdO+qINfL/kwCADQAmCJHcwBVq5cWSCquoD7BGTZRT+GZP9" +
            "t77///u533nnnE1kE+eQR3IFXGAXgczmYxpDBCNAs48mGQ4cOnQj3JWIXvcfx48f/LSO3aknbcOGHAqBDa+/3SQJhWEnDDhCANxKsXr26oLKy8mxbW5ubDPZTNDc3d7711ltbdu3ahc" +
            "emwJt2Zs0vzIiAAqAqSD4NOxoBSEZ5pqio6LSML90o0E8hyfrZLVu2nCotLcX1fsObGB0Z5iWexhyAIqA6SD7NdAU7d+48KZ9x2s0F+hcwW4+7uD/88MO9kqshWWfip00LgCIwLws0j" +
            "wnbBkE4DdvAordv335+xowZyaNHjx6bmJj4+bhB/3OADkF1dXX9kiVLXiwrK8PVP9ztwxyAeQCMImAEMARTETCGCq0c7Ii7SIxt27atUPoY981O/Qji/c3vvvvu+42X7xUD4bzrh6T7" +
            "iwDG+CIoHQUYCXSdkSDyxIkT9SK4hvz8/CmpqakJfMjDxbUB5vpPnz5des8997wqQ0DkZ+gCIARnBNACALxdAAAWTYOCJt8rABkFRFy4cKFFhoYlCxcunB0VZd437OIaQBwRD6acePL" +
            "JJ//n448/xguVQD7Dv773T0cAb/gHKACAHu80LQDTJmNM3P7ceNNNN43Izs7OtF9C6KKPcebMmbNI/J577rk9Ev2R+TeI+SMfRu+nAIwInAIgWAexWgTG2tvbIxoaGjpjY2Obpk+fPj" +
            "4hISHeFUHfApNze/fu3fPXv/511/79+/FQJTxfez/CPj1fe78PNGlaGUwSsBMMB2B/AmuSoWDjCy+8cEg+/BCSD4QjF30DOfedxcXFpwoKCg6+/fbbeFmQSdDFyI8O+U7P94GOAIAJ8" +
            "coAiITLznpUYWFhxaJFiyYmC6Kjo53HcxEGwPvXrVv37muvvXa4trYWoR9er4d+2vNhWgA+nupPABpcJuk0IrKkpKSlqqqqQkYFw2VUkC49gV7vIsSA9z/77LOvrVmz5p+45CtN6PdJ" +
            "vhaA7vdZAj5RwCkAZ4gAmWhzkkohGDt+/Hid5AW1w4YNS5CkMPDD/S56jT/+8Y/rX3rppY8k+6/q7Oxk0kdj36+9H/xBAICP9wM9CdmadI1I6f8jpCuozcrKivB4PKn279S4CCHE81t" +
            "37dp14LHHHntH+v8qcTjt+YH6f4Z8ll0QSACaaGdJcBtYJPLAysrKura2tpr09PTBIgb3B6dCBPTzBw8ePPbUU0/9r4z3cUUW43095ONsLUoQz36fBjiju0FPI4Cz9DERQEt5eXm9EF" +
            "8/cuTILMkJUtycoHeorq6+cPTo0SPr16//YO3atUfE8znZA6Png3g97GOfT9L9kg8EKwBNtBN6ZBAhX7hVhoZVeXl5cbm5uUPj4+MT3DmCqwM8H+T/7W9/2/XMM8/glSkgnaEf5NPrN" +
            "fn0epCuzS+CEUAg4glNrtmupaWlc9OmTZ/OnDlzCCJBrECiQnfHcOEA+nyEfXi+Ih8hnyX7/Ct5fkDygWAFgIOQQJS67gS3xd0px6dNm5Yio4OMxMTEK7/NwIUXSPjQ5yPsyyJJ13P9" +
            "JJ/E0xgBgkIwAvASapYug3VdaqMKIzZv3lxYV1dXJklhYkZGRpabE3QPjPMx1EO2j4TP7vMZ9mkM/drzdfgHyEW3CDYHMGRernaB/hDUSb4B7lSRIWJNRUVFmfQCDTk5OWPi4uLcnMA" +
            "BEI8ZPkzyYJyPoZ6d7ZN0eD1M9/va+3tMPhCsAIBAB9TCcG5jlmWI2Cajg4bS0tKqS5culQ4dOjQnOTk50Y0GlwHiT5w4cRzTu5jhwySPPc7XCZ/u8/2Rz3OPMhBXXdATAQDBEKa/AO" +
            "udQnx7WVlZY1FR0fno6Ogaj8eTkJKS4sH1gy9qfogLaLiki6t6kunvwNw+pnftGT56PbN9fwmfJl9b0OipAIBgRaBhvpiEtPZz5841b9mypTQ7O7sTL1TCMFFEEPdFu7EEd/LgZg5cz" +
            "8cl3VdeeeWYfWEH5Otw7/T6kJEPXI0ACAoBpf5gf19Cf0FjH3zwQdmePXuKJ06cKMHAkxwTExMtkSDq89wt4O5d8fqO5ubmJtzGhTt5cDOHfT2fpGvPJ/H0fBLvJB9g2SP0RgD4QJKv" +
            "RYBSJyMs0aa/eIfkBU1vvPHG4ZqamrMJCQkR6enpQ2W4GNbfK7yWkETP3L27YcOG/8M9fLiNy76Th4keyYfB8ykCf9k+zqG2q0IovI2hG8dCHYY6iIThhXY08x5CMf5UvSnF+xPQFeT" +
            "m5qY+/fTT986bNy9fhBDzeckN8MQOHtpAuH/ooYfeww00DQ0NyPDp6UzwtIF0f/09DegV+UCozrBTBCwpAhiijRYCfmwHP4lFQcRKHhA/duzYNIkEyb/4xS9unzp16o1paWmpA3XYiA" +
            "c18aweHtfCEzt4aENCP94hD4Lp5U7SA3l8yMkHQuliThEAFAHI1yVIJ/na0IbIELdgwYJheXl5g+fMmTN28uTJ148aNWpMZmZmaN+7Hga0tLS04fl8PKKNp3TxoCae1bMf1yLR2ut1H" +
            "w+yWWrPJ9khJR8IpQB4LJSsQwAwEK/FoCOBs0Q0oDjiZs+enTFjxozs8ePHZ0oXkSHlZBHC0EGDBiVK10GhXVPghUx4Jw9ey4I3c2Aoh+fz8Yi2/ZSuJpnEk3y0aY9HncTT40k2iQ8J" +
            "+UAoBQDo41EIFAGNUQBGIWgD8TQIwbQJ2bHjxo3zLF++fPqECRNwO/pgQVpSUpJHuohBkkPExcbGhl0Q7e3tZjgrxDbjPXyCCryNCy9kwjt58FoW+80cIJdEg1RNNgUBc3o7DMsgmeR" +
            "r0kNGPhBqAWiQDIpAi8HZJYBkioLka0FoYZh2DB+XLVs2Pj8/f/zIkSOH4S6k1NRUzCfEylDSDCkFUkREIpnsyfDSHq5JcXnYJmW7FG1irUJ8q3h8E97AefTo0cLXX3/9YwnxlY63cc" +
            "Ho4Zp4GrYLRD6JpwFaACFFOAUAaI+8khBY1+TDtEC4Trd5be7cuUPuvvvuGydNmjRmiEAEkYq7lSV6RKWkpAQ9qsBLsjF9LcO2dhm2VeN9+4WFhcXbtm07VlBQUFJUVIT+XJNNInUbj" +
            "CRzG2dJbyfZmnwibOQD4RYAgc/RBmgRsA4isaxJpjh0G4TAdu8+cHlMKEkRLR6PSSVzXEQC8WLU8dkoA4EnG7+c1iHh3gQAWW4XMeCtae1I8mQZBIFEEKhLrKOBfGyHkkTDNOmok3CS" +
            "jLouwwqS0RfQ5AMkwpBkG4hESVK1oc2f+dtHH5Oks+78HhokgsZlEIYShJE01rGOItAE07gdjfuidH4egfY+QaATEU44CXASg2VNpLNOojXhNG6DUps+Ps0fSAiAksv+iENdE6zrer1" +
            "zPxpJ5+cBaO9TBDoR4QY/10mGJssfkZpkml7HZezPdtZ5XFggkAwS4yTNaSRck65LvR2PB7Dk5xC63ifo7mT0Bfj5/kpNIJc14f4M6wPtCyPQ5oSTCJBEgkikrtOcZDvNeSzAWb9m8H" +
            "cirgVIlIYmkEYiuU7XneLgPgDbCL2OAEkECWKbk0x/xnV6G4BtMA2uv4awrP8HXAan3EbypX8AAAAASUVORK5CYII=");
    }
}
