using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MazeSimulator
{
    /// <summary>
    /// Wrapper class than encapsulates Experiment objects
    /// </summary>
    public class ExperimentWrapper
    {
        #region XML serialization
        [
            System.Xml.Serialization.XmlElement(typeof(SimulatorExperiment)),
            System.Xml.Serialization.XmlElement(typeof(MultiAgentExperiment))
        ]
        #endregion

        #region Instance variables

        public SimulatorExperiment experiment;

        #endregion

        #region Constructors

        /// <summary>
        ///  Creates an empty ExperimentWrapper object.
        /// </summary>
        public ExperimentWrapper()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Saves this ExperimentWrapper object as an XML file.
        /// </summary>
        public virtual void save(string name)
        {
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(this.GetType());
            TextWriter outfile = new StreamWriter(name);
            x.Serialize(outfile, this);
            outfile.Close();
        }

        /// <summary>
        /// Loads a CurrentEnvironment from an XML file.
        /// </summary>
        public static ExperimentWrapper load(string name)
        {
            //TODO include LEO

            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(ExperimentWrapper));
            TextReader infile = new StreamReader(name);
            ExperimentWrapper e = (ExperimentWrapper)x.Deserialize(infile);
            infile.Close();
            return e;
        }

        #endregion
    }
}
