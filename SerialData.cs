using System;
using System.Runtime.InteropServices;


namespace PSR
{
    class SerialDataStruct
    {
        enum TableId : uint
        {
            CellGroupCount = 0,
            ParallelSeries = 0x0010,
            CurrentLimits = 0x0011,
            VoltageLimits = 0x0012,
            VoltageWarningLimits = 0x0013,
            CurrentWarningLimits = 0x0014,

            VoltageResistanceCharacterization = 0x0101,
            VoltageWattHourCharacterization = 0x0100,

            ContactorConfiguration = 0x0200,
            PrechargeConfiguration = 0x0201,

            TemperatureProbeConfiguration = 0x0300,
        };

        struct Table
        {
            TableId Id;
            short Offset;
        };

        struct ConfigHeader
        {
            uint CheckCode;     // CRC32
            uint Size;          // Size of the configuration in bytes
            uint ConfigVersion; // Version of the configuration (incremented on every change)
            uint TableCount;    // Number of tables in the configuration
            uint Reserved;      // Reserved for future use
            uint Timestamp;     // Unix Timestamp of config creation
            Table[] Tables;
        };

        struct ParallelSeries
        {
            byte Parallel;
            byte Series;
        };

        struct CurrentLimits
        {
            float MaxCharging;
            float MaxChargingTemperature;
            float MaxDischarging;
            float MaxDischargingTemperature;
        };

        struct CurrentWarningLimits
        {
            float MaxChargingWarning;
            float MaxChargingTemperatureWarning;
            float MaxDischargingWarning;
            float MaxDischargingTemperatureWarning;
        };

        struct VoltageLimits
        {
            float MaxCellVoltage;
            float MinCellVoltage;
            float MaxCellVoltageCharging;

            float MaxPackVoltage;
            float MinPackVoltage;
            float MaxPackVoltageCharging;
        };

        struct VoltageWarningLimits
        {
            float MaxCellWarning;
            float MinCellWarning;
            float MaxCellWarningCharging;

            float MaxPackWarning;
            float MinPackWarning;
            float MaxPackWarningCharging;
        };

        struct ContactorConfiguration
        {
            struct Contactor
            {
                bool Enabled;
                uint HoldDelayMs;
                float HoldDutyCycle;
                float PullInDutyCycle;
            };

            uint ContactorPwmFrequency;
            Contactor MainHighSide;
            Contactor MainLowSide;
            Contactor Charge;
            Contactor Precharge;
        };

        struct PrechargeConfiguration
        {
            bool InvertSignal;
            float Delay;
        }
    }

     namespace Characterization
     {
        struct VoltageWattHoursTable
        { 
            uint Length;
            float Current;
            float StartVoltage;
            float Step;
            float[] WattHours;

            private static bool TryWrite(Span<byte> buffer)
            {
                return false;
            }
            static bool TryRead(Span<byte> buffer, out VoltageWattHoursTable characterization)
            {
                characterization = new VoltageWattHoursTable();
                return true;
            }
        

            struct VoltageResistanceTable
            {
                uint Length;
                float Current;
                float StartVoltage;
                float Step;
                float[] Resistance;

                private static bool TryWrite(Span<byte> buffer)
                {
                    return false;
                }
                
                private static bool TryRead(ReadOnlySpan<byte> buffer, out VoltageResistanceTable characterization)
                {
                    characterization = default;
                    return false;
                }
            };
        } // namespace Characterization
     } // namespace PSR
}