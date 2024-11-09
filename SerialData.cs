using BMSCharacterization;
using BMSManagerRebuilt;
using Microsoft.Extensions.Configuration;
using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace BMSDataStruct
{
	struct SerialStruct
	{
		ConfigHeader header;
		ParallelSeries parallelSeries;
		CurrentLimits currentLimits;
		CurrentWarningLimits currentWarningLimits;
		VoltageLimits voltageLimits;
		VoltageWarningLimits voltageWarningLimits;
		ContactorConfiguration contactorConfiguration;
		PrechargeConfiguration prechargeConfiguration;
		VoltageWattHoursTable voltageWattHoursTable;
		VoltageResistanceTable voltageResistanceTable;
	}

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

	public struct ParallelSeries(byte parallel, byte series) : IConfigEntry<ParallelSeries>
	{
		public byte Parallel { get; set; } = parallel;
		public byte Series { get; set; } = series;

		public static int Size => 2;

		public bool TryWrite(Span<byte> buffer, out int written)
		{
			if (buffer.Length < Size)
			{
				written = 0;
				return false;
			}

			buffer[0] = Parallel;
			buffer[1] = Series;

			written = Size;
			return true;
		}

		public static bool TryRead(ReadOnlySpan<byte> buffer, out ParallelSeries value)
		{
			if (buffer.Length < Size)
			{
				value = default;
				return false;
			}

			value = new ParallelSeries(buffer[0], buffer[1]);
			return true;
		}

	};

	public struct CurrentLimits(float maxCharging, float maxChargeTemp, float maxDischarge, float maxDischargeTemp) : IConfigEntry<CurrentLimits>
	{
		public float MaxCharging { get; set; } = maxCharging;
		public float MaxChargingTemperature { get; set; } = maxChargeTemp;
		public float MaxDischarging { get; set; } = maxDischarge;
		public float MaxDischargingTemperature { get; set; } = maxDischargeTemp;

		public static int Size => 4 * sizeof(float);

		public bool TryWrite(Span<byte> buffer, out int written)
		{
			if (buffer.Length < Size)
			{
				written = 0;
				return false;
			}

			BinaryPrimitives.WriteSingleLittleEndian(buffer, MaxCharging);
			BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(sizeof(float)), MaxChargingTemperature);
			BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(2 * sizeof(float)), MaxDischarging);
			BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(3 * sizeof(float)), MaxDischargingTemperature);

			written = Size;
			return true;
		}

		public static bool TryRead(ReadOnlySpan<byte> buffer, out CurrentLimits value)
		{
			if (buffer.Length < Size)
			{
				value = default;
				return false;
			}

			float maxCharging = BinaryPrimitives.ReadSingleLittleEndian(buffer);
			float maxChargeTemp = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(sizeof(float)));
			float maxDischarging = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(2 * sizeof(float)));
			float maxDischargeTemp = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(3 * sizeof(float)));

			value = new CurrentLimits(maxCharging, maxChargeTemp, maxDischarging, maxDischargeTemp);
			return true;
		}
	};

	struct CurrentWarningLimits(float maxChargingWarning, float maxChargingTemperatureWarning, float maxDischargingWarning, float maxDischargingTemperatureWarning) : IConfigEntry<CurrentWarningLimits>
	{
		float MaxChargingWarning { get; set; } = maxChargingWarning;
		float MaxChargingTemperatureWarning { get; set; } = maxChargingTemperatureWarning;
		float MaxDischargingWarning { get; set; } = maxDischargingWarning;
		float MaxDischargingTemperatureWarning { get; set; } = maxDischargingTemperatureWarning;

		public static int Size = 4 * sizeof(float);

        public bool TryWrite(Span<byte> buffer, out int written)
        {
            if (buffer.Length < Size)
            {
                written = 0;
                return false;
            }

            BinaryPrimitives.WriteSingleLittleEndian(buffer, MaxDischargingWarning);
            BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(sizeof(float)), MaxChargingTemperatureWarning);
            BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(2 * sizeof(float)), MaxDischargingWarning);
            BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(3 * sizeof(float)), MaxDischargingTemperatureWarning);

            written = Size;
            return true;
        }

        public static bool TryRead(ReadOnlySpan<byte> buffer, out CurrentWarningLimits value)
        {
            if (buffer.Length < Size)
            {
                value = default;
                return false;
            }

            float maxChargingWarning = BinaryPrimitives.ReadSingleLittleEndian(buffer);
            float maxChargeTempWarning = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(sizeof(float)));
            float maxDischargingWarning = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(2 * sizeof(float)));
            float maxDischargeTempWarning = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(3 * sizeof(float)));

            value = new CurrentWarningLimits(maxChargingWarning, maxChargeTempWarning, maxDischargingWarning, maxDischargeTempWarning);
            return true;
        }
    };

	struct VoltageLimits(float maxCellVoltage, float minCellVoltage, float maxCellVoltageCharging, float maxPackVoltage, float minPackVoltage, float maxPackVoltageCharging) : IConfigEntry<VoltageLimits>
	{
		float MaxCellVoltage { get; set; } = maxCellVoltage;
		float MinCellVoltage { get; set; } = minCellVoltage;
		float MaxCellVoltageCharging { get; set; } = maxCellVoltageCharging;

		float MaxPackVoltage { get; set; } = maxPackVoltage;
		float MinPackVoltage { get; set; } = minPackVoltage;
		float MaxPackVoltageCharging { get; set; } = maxPackVoltageCharging;

		public static int Size = 6 * sizeof(float);

        public bool TryWrite(Span<byte> buffer, out int written)
        {
            if (buffer.Length < Size)
            {
                written = 0;
                return false;
            }

            BinaryPrimitives.WriteSingleLittleEndian(buffer, MaxCellVoltage);
            BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(sizeof(float)), MinCellVoltage);
            BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(2 * sizeof(float)), MaxCellVoltageCharging);
            BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(3 * sizeof(float)), MaxPackVoltage);
            BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(4 * sizeof(float)), MinPackVoltage);
            BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(5 * sizeof(float)), MaxPackVoltageCharging);

            written = Size;
            return true;
        }

        public static bool TryRead(ReadOnlySpan<byte> buffer, out VoltageLimits value)
        {
            if (buffer.Length < Size)
            {
                value = default;
                return false;
            }

            float maxCellVoltage = BinaryPrimitives.ReadSingleLittleEndian(buffer);
            float minCellVoltage = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(sizeof(float)));
            float maxCellVoltageCharging = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(2 * sizeof(float)));
            float maxPackVoltage = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(3 * sizeof(float)));
            float minPackVoltage = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(4 * sizeof(float)));
            float maxPackVoltageCharging = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(5 * sizeof(float)));

            value = new VoltageLimits(maxCellVoltage, minCellVoltage, maxCellVoltageCharging, maxPackVoltage, minPackVoltage, maxPackVoltageCharging);
            return true;
        }
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

	struct Contactor
	{
		bool Enabled;
		uint HoldDelayMs;
		float HoldDutyCycle;
		float PullInDutyCycle;
	};

	struct ContactorConfiguration
	{


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
	};
}

namespace BMSCharacterization
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
	}
}// namespace Characterization

