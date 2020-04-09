using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UnityTelnet
{
    /// <summary>
    /// Contains the valid commands for the Telnet Server.
    /// Each method for a command is strictly named what the command is.
    /// Any command that has an option for setting is sent as a commandParam.
    /// </summary>
    public class TelnetCommandController : MonoBehaviour
    {
        /// <summary>
        /// Looks for a command to fire based on a comparison to the method name
        /// (i.e. type 'spawncube5' to have the method 'spawncube' spawn 5 cubes).
        /// </summary>
        /// <param name="command">A TelnetCommand constructed in TelnetServer.cs</param>
        /// <returns>A message relevant to the command.</returns>
        public string FireCommand(TelnetCommand command)
        {
            string commandMessage = string.Empty;

            Type thisType = this.GetType();

            MethodInfo commandMethod = thisType.GetMethod(command.commandName,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            object returnObject = commandMethod.Invoke(this, new object[] { command.commandParam });

            return returnObject.ToString();
        }

        #region Commands
        /// <summary>
        /// Spawns cubes into the scene.
        /// </summary>
        /// <param name="cp"></param>
        /// <returns>Number of cubes in the scene.</returns>
        public string spawncube(int? cp)
        {
            if (cp != null)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => MakeCubes((int)cp));
                return "* Spawned " + cp + " Cubes\r\n";
            }
            else
            {
                return "* There are " + CubesSpawned.Count + " Cubes in the scene\r\n";
            }
        } 
        #endregion

        #region ExampleCubeSpawner
        List<GameObject> CubesSpawned = new List<GameObject>();
        public void MakeCubes(int count)
        {
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    /// Creates a cube and places it in a random 50^3m area
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    int randX = UnityEngine.Random.Range(0, 50);
                    int randY = UnityEngine.Random.Range(0, 50);
                    int randZ = UnityEngine.Random.Range(0, 50);
                    go.transform.position = new Vector3(randX, randY, randZ);
                    CubesSpawned.Add(go);
                }
            }
            else
            {
                print("No cubes to spawn!");
            }
        }
        #endregion
    }
}
