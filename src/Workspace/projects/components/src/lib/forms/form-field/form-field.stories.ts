import type { Meta, StoryObj } from '@storybook/angular';
import { FormField } from './form-field';

const meta: Meta<FormField> = {
  title: 'Components/Forms/FormField',
  component: FormField,
  tags: ['autodocs'],
  argTypes: {
    label: {
      control: 'text',
      description: 'Field label text',
    },
    hint: {
      control: 'text',
      description: 'Hint text shown below the field',
    },
    error: {
      control: 'text',
      description: 'Error message (replaces hint when present)',
    },
    required: {
      control: 'boolean',
      description: 'Shows required indicator',
    },
  },
};

export default meta;
type Story = StoryObj<FormField>;

export const Default: Story = {
  args: {
    label: 'Field Label',
    hint: 'This is a helpful hint',
    error: '',
    required: false,
  },
  render: (args) => ({
    props: args,
    template: `
      <g-form-field [label]="label" [hint]="hint" [error]="error" [required]="required">
        <input type="text" placeholder="Enter value" style="width: 100%; padding: 8px; border: 1px solid #424242; border-radius: 4px; background: #1e1e1e; color: white;" />
      </g-form-field>
    `,
  }),
};

export const Required: Story = {
  args: {
    label: 'Required Field',
    hint: 'This field is required',
    error: '',
    required: true,
  },
  render: (args) => ({
    props: args,
    template: `
      <g-form-field [label]="label" [hint]="hint" [error]="error" [required]="required">
        <input type="text" placeholder="Enter value" style="width: 100%; padding: 8px; border: 1px solid #424242; border-radius: 4px; background: #1e1e1e; color: white;" />
      </g-form-field>
    `,
  }),
};

export const WithError: Story = {
  args: {
    label: 'Field with Error',
    hint: 'This hint is replaced by error',
    error: 'This field has an error',
    required: true,
  },
  render: (args) => ({
    props: args,
    template: `
      <g-form-field [label]="label" [hint]="hint" [error]="error" [required]="required">
        <input type="text" placeholder="Enter value" style="width: 100%; padding: 8px; border: 1px solid #f44336; border-radius: 4px; background: #1e1e1e; color: white;" />
      </g-form-field>
    `,
  }),
};
