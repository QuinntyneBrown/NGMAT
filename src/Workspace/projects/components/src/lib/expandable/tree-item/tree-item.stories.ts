import { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { MatIconModule } from '@angular/material/icon';
import { TreeItem, TreeItemNode } from './tree-item';

const meta: Meta<TreeItem> = {
  title: 'Components/Expandable/TreeItem',
  component: TreeItem,
  tags: ['autodocs'],
  decorators: [
    moduleMetadata({
      imports: [MatIconModule, TreeItem],
    }),
  ],
  argTypes: {
    label: {
      control: 'text',
    },
    icon: {
      control: 'text',
    },
    expanded: {
      control: 'boolean',
    },
    level: {
      control: 'number',
    },
    expandedChange: { action: 'expandedChange' },
    itemClick: { action: 'itemClick' },
  },
};

export default meta;
type Story = StoryObj<TreeItem>;

export const Default: Story = {
  args: {
    label: 'Tree Item',
    icon: 'folder',
    expanded: false,
    level: 0,
    children: [],
  },
  render: (args) => ({
    props: args,
    template: `
      <g-tree-item
        [label]="label"
        [icon]="icon"
        [expanded]="expanded"
        [level]="level"
        [children]="children"
        (expandedChange)="expandedChange($event)"
        (itemClick)="itemClick()">
      </g-tree-item>
    `,
  }),
};

export const WithChildren: Story = {
  args: {
    label: 'Parent Folder',
    icon: 'folder',
    expanded: true,
    level: 0,
    children: [
      { label: 'Child 1', icon: 'description' },
      { label: 'Child 2', icon: 'description' },
      { label: 'Child 3', icon: 'description' },
    ] as TreeItemNode[],
  },
  render: (args) => ({
    props: args,
    template: `
      <g-tree-item
        [label]="label"
        [icon]="icon"
        [expanded]="expanded"
        [level]="level"
        [children]="children"
        (expandedChange)="expandedChange($event)"
        (itemClick)="itemClick()">
      </g-tree-item>
    `,
  }),
};

export const NestedTree: Story = {
  args: {
    label: 'Root',
    icon: 'folder',
    expanded: true,
    level: 0,
    children: [
      {
        label: 'src',
        icon: 'folder',
        expanded: true,
        children: [
          {
            label: 'components',
            icon: 'folder',
            expanded: true,
            children: [
              { label: 'button.ts', icon: 'code' },
              { label: 'input.ts', icon: 'code' },
            ],
          },
          { label: 'main.ts', icon: 'code' },
          { label: 'styles.scss', icon: 'style' },
        ],
      },
      {
        label: 'assets',
        icon: 'folder',
        children: [
          { label: 'logo.png', icon: 'image' },
          { label: 'icon.svg', icon: 'image' },
        ],
      },
      { label: 'package.json', icon: 'settings' },
      { label: 'README.md', icon: 'description' },
    ] as TreeItemNode[],
  },
  render: (args) => ({
    props: args,
    template: `
      <div style="width: 300px; border: 1px solid #e0e0e0; border-radius: 4px; padding: 8px;">
        <g-tree-item
          [label]="label"
          [icon]="icon"
          [expanded]="expanded"
          [level]="level"
          [children]="children"
          (expandedChange)="expandedChange($event)"
          (itemClick)="itemClick()">
        </g-tree-item>
      </div>
    `,
  }),
};

export const Collapsed: Story = {
  args: {
    label: 'Collapsed Folder',
    icon: 'folder',
    expanded: false,
    level: 0,
    children: [
      { label: 'Hidden Child 1', icon: 'description' },
      { label: 'Hidden Child 2', icon: 'description' },
    ] as TreeItemNode[],
  },
  render: (args) => ({
    props: args,
    template: `
      <g-tree-item
        [label]="label"
        [icon]="icon"
        [expanded]="expanded"
        [level]="level"
        [children]="children"
        (expandedChange)="expandedChange($event)"
        (itemClick)="itemClick()">
      </g-tree-item>
    `,
  }),
};

export const LeafNode: Story = {
  args: {
    label: 'document.txt',
    icon: 'description',
    expanded: false,
    level: 0,
    children: [],
  },
  render: (args) => ({
    props: args,
    template: `
      <g-tree-item
        [label]="label"
        [icon]="icon"
        [expanded]="expanded"
        [level]="level"
        [children]="children"
        (expandedChange)="expandedChange($event)"
        (itemClick)="itemClick()">
      </g-tree-item>
    `,
  }),
};
