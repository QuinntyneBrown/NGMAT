import { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { MatCardModule } from '@angular/material/card';
import { PropertyPanel, Property } from './property-panel';

const meta: Meta<PropertyPanel> = {
  title: 'Components/Content/PropertyPanel',
  component: PropertyPanel,
  tags: ['autodocs'],
  decorators: [
    moduleMetadata({
      imports: [MatCardModule],
    }),
  ],
  argTypes: {
    title: {
      control: 'text',
    },
    properties: {
      control: 'object',
    },
  },
};

export default meta;
type Story = StoryObj<PropertyPanel>;

export const Default: Story = {
  args: {
    title: 'Orbital Parameters',
    properties: [
      { label: 'Semi-major Axis', value: '6778.14', unit: 'km' },
      { label: 'Eccentricity', value: '0.0001' },
      { label: 'Inclination', value: '51.64', unit: 'deg' },
      { label: 'Period', value: '92.68', unit: 'min' },
    ] as Property[],
  },
};

export const WithoutTitle: Story = {
  args: {
    title: '',
    properties: [
      { label: 'Latitude', value: '28.5729', unit: 'N' },
      { label: 'Longitude', value: '-80.6490', unit: 'W' },
      { label: 'Altitude', value: '408', unit: 'km' },
    ] as Property[],
  },
};

export const SpacecraftProperties: Story = {
  args: {
    title: 'Spacecraft Configuration',
    properties: [
      { label: 'Mass', value: '420,000', unit: 'kg' },
      { label: 'Length', value: '109', unit: 'm' },
      { label: 'Width', value: '73', unit: 'm' },
      { label: 'Solar Array Area', value: '2,500', unit: 'm2' },
      { label: 'Pressurized Volume', value: '916', unit: 'm3' },
    ] as Property[],
  },
};

export const TelemetryData: Story = {
  args: {
    title: 'Real-time Telemetry',
    properties: [
      { label: 'Battery Level', value: '87', unit: '%' },
      { label: 'Solar Panel Output', value: '12.4', unit: 'kW' },
      { label: 'Internal Temperature', value: '22.5', unit: 'C' },
      { label: 'Signal Strength', value: '-68', unit: 'dBm' },
      { label: 'Data Rate', value: '150', unit: 'Mbps' },
    ] as Property[],
  },
};

export const SideBySide: Story = {
  render: () => ({
    template: `
      <div style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 16px; max-width: 700px;">
        <g-property-panel
          title="Position"
          [properties]="[
            { label: 'X', value: '1234.56', unit: 'km' },
            { label: 'Y', value: '-789.12', unit: 'km' },
            { label: 'Z', value: '345.67', unit: 'km' }
          ]"
        ></g-property-panel>
        <g-property-panel
          title="Velocity"
          [properties]="[
            { label: 'Vx', value: '7.234', unit: 'km/s' },
            { label: 'Vy', value: '-0.456', unit: 'km/s' },
            { label: 'Vz', value: '1.789', unit: 'km/s' }
          ]"
        ></g-property-panel>
      </div>
    `,
  }),
};
