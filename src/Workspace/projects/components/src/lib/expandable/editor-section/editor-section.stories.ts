import { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { MatIconModule } from '@angular/material/icon';
import { EditorSection } from './editor-section';

const meta: Meta<EditorSection> = {
  title: 'Components/Expandable/EditorSection',
  component: EditorSection,
  tags: ['autodocs'],
  decorators: [
    moduleMetadata({
      imports: [MatIconModule],
    }),
  ],
  argTypes: {
    title: {
      control: 'text',
    },
    icon: {
      control: 'text',
    },
    expanded: {
      control: 'boolean',
    },
    expandedChange: { action: 'expandedChange' },
  },
};

export default meta;
type Story = StoryObj<EditorSection>;

export const Default: Story = {
  args: {
    title: 'Properties',
    icon: '',
    expanded: true,
  },
  render: (args) => ({
    props: args,
    template: `
      <g-editor-section
        [title]="title"
        [icon]="icon"
        [expanded]="expanded"
        (expandedChange)="expandedChange($event)">
        <p>Section content goes here.</p>
      </g-editor-section>
    `,
  }),
};

export const WithIcon: Story = {
  args: {
    title: 'Transform',
    icon: 'transform',
    expanded: true,
  },
  render: (args) => ({
    props: args,
    template: `
      <g-editor-section
        [title]="title"
        [icon]="icon"
        [expanded]="expanded"
        (expandedChange)="expandedChange($event)">
        <div style="display: grid; gap: 8px;">
          <div>Position: X: 0, Y: 0, Z: 0</div>
          <div>Rotation: X: 0, Y: 0, Z: 0</div>
          <div>Scale: X: 1, Y: 1, Z: 1</div>
        </div>
      </g-editor-section>
    `,
  }),
};

export const Collapsed: Story = {
  args: {
    title: 'Materials',
    icon: 'palette',
    expanded: false,
  },
  render: (args) => ({
    props: args,
    template: `
      <g-editor-section
        [title]="title"
        [icon]="icon"
        [expanded]="expanded"
        (expandedChange)="expandedChange($event)">
        <p>Material properties would go here.</p>
      </g-editor-section>
    `,
  }),
};

export const MultipleSections: Story = {
  args: {
    title: 'Section',
    icon: '',
    expanded: true,
  },
  render: (args) => ({
    props: args,
    template: `
      <div style="width: 300px;">
        <g-editor-section title="Transform" icon="transform" [expanded]="true">
          <div>Position, Rotation, Scale controls</div>
        </g-editor-section>
        <g-editor-section title="Materials" icon="palette" [expanded]="true">
          <div>Material properties</div>
        </g-editor-section>
        <g-editor-section title="Physics" icon="speed" [expanded]="false">
          <div>Physics properties</div>
        </g-editor-section>
        <g-editor-section title="Scripts" icon="code" [expanded]="false">
          <div>Attached scripts</div>
        </g-editor-section>
      </div>
    `,
  }),
};
