import { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { Dialog } from './dialog';

const meta: Meta<Dialog> = {
  title: 'Components/Overlay/Dialog',
  component: Dialog,
  tags: ['autodocs'],
  decorators: [
    moduleMetadata({
      imports: [MatDialogModule, MatButtonModule],
    }),
  ],
  argTypes: {
    title: {
      control: 'text',
    },
  },
};

export default meta;
type Story = StoryObj<Dialog>;

export const Default: Story = {
  args: {
    title: 'Dialog Title',
  },
  render: (args) => ({
    props: args,
    template: `
      <g-dialog [title]="title">
        <p>This is the dialog content. You can put any content here.</p>
        <div dialogFooter>
          <button mat-button>Cancel</button>
          <button mat-raised-button color="primary">Confirm</button>
        </div>
      </g-dialog>
    `,
  }),
};

export const WithHeaderSlot: Story = {
  args: {
    title: 'Dialog with Custom Header',
  },
  render: (args) => ({
    props: args,
    template: `
      <g-dialog [title]="title">
        <span dialogHeader style="color: #666; font-size: 12px;">Optional subtitle</span>
        <p>Dialog content with a custom header element.</p>
        <div dialogFooter>
          <button mat-button>Close</button>
        </div>
      </g-dialog>
    `,
  }),
};

export const NoTitle: Story = {
  args: {
    title: '',
  },
  render: (args) => ({
    props: args,
    template: `
      <g-dialog [title]="title">
        <p>This dialog has no title header.</p>
        <div dialogFooter>
          <button mat-raised-button color="primary">OK</button>
        </div>
      </g-dialog>
    `,
  }),
};

export const LongContent: Story = {
  args: {
    title: 'Scrollable Dialog',
  },
  render: (args) => ({
    props: args,
    template: `
      <g-dialog [title]="title">
        <p>This dialog has long content that will scroll.</p>
        <p *ngFor="let i of [1,2,3,4,5,6,7,8,9,10]">
          Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.
        </p>
        <div dialogFooter>
          <button mat-button>Cancel</button>
          <button mat-raised-button color="primary">Save</button>
        </div>
      </g-dialog>
    `,
  }),
};
