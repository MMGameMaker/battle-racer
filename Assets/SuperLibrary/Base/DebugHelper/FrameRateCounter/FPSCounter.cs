using UnityEngine;

namespace Super
{
    public class FPSCounter : MonoBehaviour
    {

        public int frameRange = 60;
        const float fpsMeasurePeriod = 0.5f;
        private float m_FpsNextPeriod = 0;
        private int m_FpsAccumulator = 0;

        public int AverageFPS { get; private set; }
        public int HighestFPS { get; private set; }
        public int LowestFPS { get; private set; }
        public int CurentFPS { get; private set; }

        int[] fpsBuffer;
        int fpsBufferIndex;

        void Awake()
        {
            m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
        }

        void LateUpdate()
        {
            if (fpsBuffer == null || fpsBuffer.Length != frameRange)
            {
                InitializeBuffer();
            }
            UpdateBuffer();
            CalculateFPS();
        }

        void InitializeBuffer()
        {
            if (frameRange <= 0)
            {
                frameRange = 1;
            }
            fpsBuffer = new int[frameRange];
            fpsBufferIndex = 0;
        }

        void UpdateBuffer()
        {
            fpsBuffer[fpsBufferIndex++] = (int)(1f / Time.unscaledDeltaTime);
            if (fpsBufferIndex >= frameRange)
            {
                fpsBufferIndex = 0;
            }
        }

        void CalculateFPS()
        {
            int sum = 0;
            int highest = 0;
            int lowest = int.MaxValue;
            for (int i = 0; i < frameRange; i++)
            {
                int fps = fpsBuffer[i];
                sum += fps;
                if (fps > highest)
                {
                    highest = fps;
                }
                if (fps < lowest)
                {
                    lowest = fps;
                }
            }
            AverageFPS = (int)((float)sum / frameRange);
            HighestFPS = highest;
            LowestFPS = lowest;

            m_FpsAccumulator++;
            if (Time.realtimeSinceStartup > m_FpsNextPeriod)
            {
                CurentFPS = (int)(m_FpsAccumulator / fpsMeasurePeriod);
                m_FpsAccumulator = 0;
                m_FpsNextPeriod += fpsMeasurePeriod;
            }
        }
    }
}