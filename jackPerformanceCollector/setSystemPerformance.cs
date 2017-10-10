namespace jackPerformanceCollector
{
    class setSystemPerformance
    {
        public SystemPerformance getPerformance(int cpu, int memory)
        {
            SystemPerformance systemperformance = new SystemPerformance();

            systemperformance.cpu = cpu;
            systemperformance.memory = memory;
            return systemperformance;
        }
    }
}
