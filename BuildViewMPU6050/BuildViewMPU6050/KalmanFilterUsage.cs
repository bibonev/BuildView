using System;

namespace BuildViewMPU6050
{
    public class TheFilter
    {
        private float m_Top;
        private float m_Cutoff;
        private float m_Value;

        /// <summary>
        /// The filtered value.
        /// </summary>
        public float Value
        {
            get
            {
                return m_Value;
            }
        }

        /// <summary>
        /// Create an instance of a Low Pass Filter, which removes noise in favour of lag.
        /// </summary>
        /// <param name="cutoff">The amount of cutoff</param>
        public TheFilter(float cutoff) : this(cutoff, 0) { }

        /// <summary>
        /// Create an instance of a Low Pass Filter, which removes noise in favour of lag.
        /// </summary>
        /// <param name="cutoff">The amount of cutoff</param>
        /// <param name="initialState">The initial state of the filter</param>
        public TheFilter(float cutoff, float initialState)
        {
            if (cutoff > 0.5)
            {
                throw new ArgumentOutOfRangeException("cutoff should be less than 0.5");
            }
            m_Cutoff = cutoff;
            m_Top = 1 - m_Cutoff;
            m_Value = initialState;
        }

        /// <summary>
        /// Update the filter
        /// </summary>
        /// <param name="value">The new value from the input</param>
        /// <returns>The filtered value</returns>
        public float Update(float value)
        {
            m_Value = (m_Value * m_Top) + (value * m_Cutoff);
            return m_Value;
        }
    }
}
