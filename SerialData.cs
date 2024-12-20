using BMSCharacterization;
using BMSDataStruct;
using BMSManagerRebuilt;
using Microsoft.Extensions.Configuration;
using Microsoft.UI.Input;
using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Media.Capture;


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

		public int Size => 2;

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

		public bool TryRead(ReadOnlySpan<byte> buffer, out ParallelSeries value)
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

		public int Size => 4 * sizeof(float);

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

		public bool TryRead(ReadOnlySpan<byte> buffer, out CurrentLimits value)
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

	public struct CurrentWarningLimits(float maxChargingWarning, float maxChargingTemperatureWarning, float maxDischargingWarning, float maxDischargingTemperatureWarning) : IConfigEntry<CurrentWarningLimits>
	{
		float MaxChargingWarning { get; set; } = maxChargingWarning;
		float MaxChargingTemperatureWarning { get; set; } = maxChargingTemperatureWarning;
		float MaxDischargingWarning { get; set; } = maxDischargingWarning;
		float MaxDischargingTemperatureWarning { get; set; } = maxDischargingTemperatureWarning;

		public int Size => 4 * sizeof(float);

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

        public bool TryRead(ReadOnlySpan<byte> buffer, out CurrentWarningLimits value)
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

	public struct VoltageLimits(float maxCellVoltage, float minCellVoltage, float maxCellVoltageCharging, float maxPackVoltage, float minPackVoltage, float maxPackVoltageCharging) : IConfigEntry<VoltageLimits>
	{
		float MaxCellVoltage { get; set; } = maxCellVoltage;
		float MinCellVoltage { get; set; } = minCellVoltage;
		float MaxCellVoltageCharging { get; set; } = maxCellVoltageCharging;

		float MaxPackVoltage { get; set; } = maxPackVoltage;
		float MinPackVoltage { get; set; } = minPackVoltage;
		float MaxPackVoltageCharging { get; set; } = maxPackVoltageCharging;

		public int Size => 6 * sizeof(float);

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

        public bool TryRead(ReadOnlySpan<byte> buffer, out VoltageLimits value)
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

	public struct VoltageWarningLimits (float maxCellWarning, float minCellWarning, float maxCellWarningCharging, float maxPackWarning, float minPackWarning, float maxPackWarningCharging) : IConfigEntry<VoltageWarningLimits>
    {
		float MaxCellWarning { get; set; } = maxCellWarning;
		float MinCellWarning { get; set; } = minCellWarning;
		float MaxCellWarningCharging { get; set; } = maxCellWarningCharging;

		float MaxPackWarning { get; set; } = maxPackWarning;
		float MinPackWarning { get; set; } = minPackWarning;
		float MaxPackWarningCharging { get; set; } = maxPackWarningCharging;

		public int Size => 6 * sizeof(float);

        public bool TryWrite(Span<byte> buffer, out int written)
        {
            if (buffer.Length < Size)
            {
                written = 0;
                return false;
            }

            BinaryPrimitives.WriteSingleLittleEndian(buffer, MaxCellWarning);
            BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(sizeof(float)), MinCellWarning);
            BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(2 * sizeof(float)), MaxCellWarningCharging);
            BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(3 * sizeof(float)), MaxPackWarning);
            BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(4 * sizeof(float)), MinPackWarning);
            BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(5 * sizeof(float)), MaxPackWarningCharging);

            written = Size;
            return true;
        }

        public bool TryRead(ReadOnlySpan<byte> buffer, out VoltageWarningLimits value)
        {
            if (buffer.Length < Size)
            {
                value = default;
                return false;
            }

            float maxCellWarning = BinaryPrimitives.ReadSingleLittleEndian(buffer);
            float minCellWarning = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(sizeof(float)));
            float maxCellWarningCharging = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(2 * sizeof(float)));
            float maxPackWarning = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(3 * sizeof(float)));
            float minPackWarning = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(4 * sizeof(float)));
            float maxPackWarningCharging = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(5 * sizeof(float)));

            value = new VoltageWarningLimits(maxCellWarning, minCellWarning, maxCellWarningCharging, maxPackWarning, minPackWarning, maxPackWarningCharging);
            return true;
        }
    };

	public struct Contactor(bool enabled, uint holdDelaysMS, float holdDutyCycle, float pullInDutyCycle) : IConfigEntry<Contactor>
	{
		bool Enabled { get; set; } = enabled;
		uint HoldDelayMs { get; set; } = holdDelaysMS;
		float HoldDutyCycle { get; set; } = holdDutyCycle;
		float PullInDutyCycle { get; set; } = pullInDutyCycle;

		public int Size => sizeof(bool) + sizeof(uint) + sizeof(float) * 2;

        public bool TryWrite(Span<byte> buffer, out int written)
        {
            if (buffer.Length < Size)
            {
                written = 0;
                return false;
            }

			Span<byte> enableByteSpan = BitConverter.GetBytes(Enabled);     //Convert enable to byte span
            enableByteSpan.CopyTo(buffer.Slice(0, sizeof(bool)));           //Copy enableByteSpan to first part of the buffer
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(sizeof(bool)), HoldDelayMs);
            BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(sizeof(bool) + sizeof(uint)), HoldDutyCycle);
            BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(sizeof(bool) + sizeof(uint) + sizeof(float)), PullInDutyCycle);


            written = Size;
            return true;
        }

        public bool TryRead(ReadOnlySpan<byte> buffer, out Contactor value)
        {
            if (buffer.Length < Size)
            {
                value = default;
                return false;
            }

            bool enabled = BitConverter.ToBoolean(buffer.Slice(0, sizeof(bool)));
            uint holdDelaysMS = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(sizeof(bool)));
            float holdDutyCycle = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(sizeof(bool) + sizeof(uint)));
            float pullInDutyCycle = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(sizeof(bool) + sizeof(uint) + sizeof(float)));

            value = new Contactor(enabled, holdDelaysMS, holdDutyCycle, pullInDutyCycle);
            return true;
        }
    };

	struct ContactorConfiguration(uint contactorPwmFrequency, Contactor mainHighSide, Contactor mainLowSide, Contactor charge, Contactor precharge) : IConfigEntry<ContactorConfiguration>
    {
		uint ContactorPwmFrequency { get; set; } = contactorPwmFrequency;
		Contactor MainHighSide { get; set; } = mainHighSide;
		Contactor MainLowSide { get; set; } = mainLowSide;
		Contactor Charge { get; set; } = charge;
		Contactor Precharge { get; set; } = precharge;

		public int Size => sizeof(uint) + MainHighSide.Size * 4;

        public bool TryWrite(Span<byte> buffer, out int written)
        {
            if (buffer.Length < Size)
            {
                written = 0;
                return false;
            }

			int temp; //Hold temp values for how much has written to Contactor variable
            BinaryPrimitives.WriteUInt32LittleEndian(buffer, ContactorPwmFrequency);

			if (MainHighSide.TryWrite(buffer.Slice(sizeof(uint)), out temp))
			{
				if (MainLowSide.TryWrite(buffer.Slice(sizeof(uint)), out temp))
				{
					if (Charge.TryWrite(buffer.Slice(sizeof(uint)), out temp))
					{
						if (Precharge.TryWrite(buffer.Slice(sizeof(uint)), out temp))
						{
							written = Size;
							return true;
						}
						else //Can't write Precharge
						{
							written = Size - Precharge.Size; 
							return false;
						}
					}
					else //Can't write Charge
					{
						written = Size - Charge.Size * 2;
						return false;
					}
				}
				else //Can't write MainLowSide
				{
					written = Size - MainLowSide.Size * 3;
					return false;
				}
			}
			else //Can't write MainHighSide
			{
				written = sizeof(uint);
				return false;
			}
        }

        public bool TryRead(ReadOnlySpan<byte> buffer, out ContactorConfiguration value)
        {
            if (buffer.Length < Size)
            {
                value = default;
                return false;
            }

            uint contactorPwmFrequency = BinaryPrimitives.ReadUInt32LittleEndian(buffer);

			bool mainHighBool = MainHighSide.TryRead(buffer.Slice(sizeof(uint)), out Contactor mainHighSide);
            bool mainLowBool = MainLowSide.TryRead(buffer.Slice(sizeof(uint) + MainLowSide.Size), out Contactor mainLowSide);
			bool chargeBool = Charge.TryRead(buffer.Slice(sizeof(uint) + Charge.Size * 2), out Contactor charge);
			bool prechargeBool = Precharge.TryRead(buffer.Slice(sizeof(uint) + Precharge.Size * 3), out Contactor precharge);
            
			if (mainLowBool & mainHighBool & chargeBool & prechargeBool)
			{
                value = new ContactorConfiguration(contactorPwmFrequency, mainHighSide, mainLowSide, charge, precharge);
				return true;
            }
			else
			{
				value = default;
				return false;
			}
        }
	};

	struct PrechargeConfiguration(bool invertSignal, float delay) : IConfigEntry<PrechargeConfiguration>
	{
		bool InvertSignal { get; set; } = invertSignal;
		float Delay { get; set; } = delay;

		public int Size => sizeof(bool) + sizeof(float);

        public bool TryWrite(Span<byte> buffer, out int written)
        {
            if (buffer.Length < Size)
            {
                written = 0;
                return false;
            }

            Span<byte> invertSignalByteSpan = BitConverter.GetBytes(InvertSignal);     //Convert enable to byte span
            invertSignalByteSpan.CopyTo(buffer.Slice(0, sizeof(bool)));           //Copy enableByteSpan to first part of the buffer
			BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(sizeof(bool)), Delay); 

            written = Size;
            return true;
        }

        public bool TryRead(ReadOnlySpan<byte> buffer, out PrechargeConfiguration value)
        {
            if (buffer.Length < Size)
            {
                value = default;
                return false;
            }

            bool invertSignal = BitConverter.ToBoolean(buffer.Slice(0, sizeof(bool)));
            float delay = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(sizeof(bool)));

            value = new PrechargeConfiguration(invertSignal, delay);
            return true;
        }
    };
}

namespace BMSCharacterization
{
	struct VoltageWattHoursTable(uint length, float current, float startVoltage, float step, float[] wattHours) : IConfigEntry<VoltageWattHoursTable>
	{
		uint Length { get; set; } = length;
		float Current { get; set; } = current;
		float StartVoltage { get; set; } = startVoltage;
		float Step { get; set; } = step;
		float[] WattHours { get; set; } = wattHours;


		public int Size => sizeof(uint) + 4 * sizeof(float); //unfinished

        public bool TryWrite(Span<byte> buffer, out int written)
        {
            if (buffer.Length < Size)
            {
                written = 0;
                return false;
            }

			BinaryPrimitives.WriteUInt32LittleEndian(buffer, Length);
            BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(sizeof(uint)), Current);
            BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(sizeof(uint) + sizeof(float)), StartVoltage);
            BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(sizeof(uint) + 2 * sizeof(float)), Step);
            for (int i = 0; i < WattHours.Length; i++)
			{
                BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(sizeof(uint) + (3+i) * sizeof(float)), WattHours[i]);
            }

            written = Size;
            return true;
        }

        public bool TryRead(ReadOnlySpan<byte> buffer, out VoltageWattHoursTable value)
        {
            if (buffer.Length < Size)
            {
                value = default;
                return false;
            }

			uint length = BinaryPrimitives.ReadUInt32LittleEndian(buffer);
            float current = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(sizeof(uint)));
			float startVoltage = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(sizeof(uint) + sizeof(float)));
			float step = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(sizeof(uint) + sizeof(float) * 2));
			float[] wattHours = default;
			int offset = 0;

			while (true)
			{
				try
				{
					wattHours[offset] = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(sizeof(uint) + sizeof(float) * (3 + offset)));
                }
				catch (ArgumentOutOfRangeException argumentOutOfRangeException)
				{
					break;
				}
				offset++;
			}

            value = new VoltageWattHoursTable(length, current, startVoltage, step, wattHours);
            return true;
        }
    }
    struct VoltageResistanceTable(uint length, float current, float startVoltage, float step, float[] resistance)
    {
        uint Length { get; set; } = length;
        float Current { get; set; } = current;
        float StartVoltage { get; set; } = startVoltage;
        float Step { get; set; } = step;
        float[] Resistance { get; set; } = resistance;


        public int Size => sizeof(uint) + 4 * sizeof(float); //unfinished

        public bool TryWrite(Span<byte> buffer, out int written)
        {
            if (buffer.Length < Size)
            {
                written = 0;
                return false;
            }

            BinaryPrimitives.WriteUInt32LittleEndian(buffer, Length);
            BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(sizeof(uint)), Current);
            BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(sizeof(uint) + sizeof(float)), StartVoltage);
            BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(sizeof(uint) + 2 * sizeof(float)), Step);
            for (int i = 0; i < Resistance.Length; i++)
            {
                BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(sizeof(uint) + (3 + i) * sizeof(float)), Resistance[i]);
            }

            written = Size;
            return true;
        }

        public bool TryRead(ReadOnlySpan<byte> buffer, out VoltageWattHoursTable value)
        {
            if (buffer.Length < Size)
            {
                value = default;
                return false;
            }

            uint length = BinaryPrimitives.ReadUInt32LittleEndian(buffer);
            float current = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(sizeof(uint)));
            float startVoltage = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(sizeof(uint) + sizeof(float)));
            float step = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(sizeof(uint) + sizeof(float) * 2));
            float[] resistance = default;
            int offset = 0;

            while (true)
            {
                try
                {
                    resistance[offset] = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(sizeof(uint) + sizeof(float) * (3 + offset)));
                }
                catch (ArgumentOutOfRangeException argumentOutOfRangeException)
                {
                    break;
                }
                offset++;
            }

            value = new VoltageWattHoursTable(length, current, startVoltage, step, resistance);
            return true;
        }
    }
}// namespace Characterization

