import type { Meta, StoryObj } from '@storybook/angular';
import { Radio } from './radio';

const meta: Meta<Radio> = {
  title: 'Components/Forms/Radio',
  component: Radio,
  tags: ['autodocs'],
  argTypes: {
    label: {
      control: 'text',
      description: 'Radio button label',
    },
    value: {
      control: 'text',
      description: 'Radio button value',
    },
    name: {
      control: 'text',
      description: 'Radio group name',
    },
    checked: {
      control: 'boolean',
      description: 'Checked state',
    },
    disabled: {
      control: 'boolean',
      description: 'Disabled state',
    },
  },
};

export default meta;
type Story = StoryObj<Radio>;

export const Default: Story = {
  args: {
    label: 'Option A',
    value: 'a',
    name: 'options',
    checked: false,
    disabled: false,
  },
};

export const Checked: Story = {
  args: {
    label: 'Selected option',
    value: 'selected',
    name: 'options',
    checked: true,
    disabled: false,
  },
};

export const Disabled: Story = {
  args: {
    label: 'Disabled option',
    value: 'disabled',
    name: 'options',
    checked: false,
    disabled: true,
  },
};

export const PropagatorSelection: Story = {
  render: () => ({
    template: `
      <div style="display: flex; flex-direction: column; gap: 8px;">
        <g-radio label="Two-Body (Keplerian)" value="keplerian" name="propagator" [checked]="true"></g-radio>
        <g-radio label="SGP4/SDP4" value="sgp4" name="propagator"></g-radio>
        <g-radio label="Numerical (RK4)" value="rk4" name="propagator"></g-radio>
        <g-radio label="Numerical (RK78)" value="rk78" name="propagator"></g-radio>
      </div>
    `,
  }),
};

export const CoordinateFrames: Story = {
  render: () => ({
    template: `
      <div style="display: flex; flex-direction: column; gap: 8px;">
        <g-radio label="ECI (J2000)" value="eci" name="frame" [checked]="true"></g-radio>
        <g-radio label="ECEF (ITRF)" value="ecef" name="frame"></g-radio>
        <g-radio label="Perifocal (PQW)" value="pqw" name="frame"></g-radio>
      </div>
    `,
  }),
};
