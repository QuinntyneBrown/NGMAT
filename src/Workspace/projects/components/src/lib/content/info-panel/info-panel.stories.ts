import { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { InfoPanel, InfoItem } from './info-panel';

const meta: Meta<InfoPanel> = {
  title: 'Components/Content/InfoPanel',
  component: InfoPanel,
  tags: ['autodocs'],
  decorators: [
    moduleMetadata({
      imports: [MatCardModule, MatIconModule],
    }),
  ],
  argTypes: {
    title: {
      control: 'text',
    },
    items: {
      control: 'object',
    },
  },
};

export default meta;
type Story = StoryObj<InfoPanel>;

export const Default: Story = {
  args: {
    title: 'Mission Overview',
    items: [
      { icon: 'rocket_launch', label: 'Launch Vehicle', value: 'Falcon 9 Block 5' },
      { icon: 'location_on', label: 'Launch Site', value: 'Kennedy Space Center, LC-39A' },
      { icon: 'satellite_alt', label: 'Payload', value: 'Crew Dragon Endeavour' },
      { icon: 'people', label: 'Crew Size', value: '4 Astronauts' },
    ] as InfoItem[],
  },
};

export const WithoutTitle: Story = {
  args: {
    title: '',
    items: [
      { icon: 'speed', label: 'Velocity', value: '7.66 km/s' },
      { icon: 'height', label: 'Altitude', value: '408 km' },
      { icon: 'public', label: 'Orbits Today', value: '16' },
    ] as InfoItem[],
  },
};

export const GroundStation: Story = {
  args: {
    title: 'Ground Station Info',
    items: [
      { icon: 'cell_tower', label: 'Station Name', value: 'Goldstone Deep Space Complex' },
      { icon: 'location_on', label: 'Location', value: 'Mojave Desert, California' },
      { icon: 'satellite', label: 'Antenna Size', value: '70m DSN Antenna' },
      { icon: 'signal_cellular_alt', label: 'Frequency Band', value: 'X-Band / Ka-Band' },
      { icon: 'schedule', label: 'Operational Since', value: '1966' },
    ] as InfoItem[],
  },
};

export const ContactInfo: Story = {
  args: {
    title: 'Mission Control',
    items: [
      { icon: 'business', label: 'Organization', value: 'NASA Johnson Space Center' },
      { icon: 'phone', label: 'Contact', value: '+1 (281) 483-0123' },
      { icon: 'email', label: 'Email', value: 'mission-control@nasa.gov' },
      { icon: 'access_time', label: 'Operating Hours', value: '24/7 Operations' },
    ] as InfoItem[],
  },
};

export const MultipleInfoPanels: Story = {
  render: () => ({
    template: `
      <div style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 16px; max-width: 800px;">
        <g-info-panel
          title="Spacecraft"
          [items]="[
            { icon: 'satellite_alt', label: 'Name', value: 'ISS (Zarya)' },
            { icon: 'straighten', label: 'Dimensions', value: '109m x 73m' },
            { icon: 'scale', label: 'Mass', value: '420,000 kg' }
          ]"
        ></g-info-panel>
        <g-info-panel
          title="Current Status"
          [items]="[
            { icon: 'check_circle', label: 'Status', value: 'Operational' },
            { icon: 'group', label: 'Crew Aboard', value: '7 Members' },
            { icon: 'update', label: 'Days in Orbit', value: '9,125+' }
          ]"
        ></g-info-panel>
      </div>
    `,
  }),
};
