import type { Meta, StoryObj } from '@storybook/angular';
import { Avatar } from './avatar';

const meta: Meta<Avatar> = {
  title: 'Components/Utilities/Avatar',
  component: Avatar,
  tags: ['autodocs'],
  argTypes: {
    src: {
      control: 'text',
      description: 'Image source URL',
    },
    initials: {
      control: 'text',
      description: 'Initials to display when no image is provided',
    },
    size: {
      control: { type: 'select' },
      options: ['sm', 'md', 'lg', 'xl'],
      description: 'Avatar size',
    },
  },
};

export default meta;
type Story = StoryObj<Avatar>;

export const Default: Story = {
  args: {
    size: 'md',
  },
};

export const WithInitials: Story = {
  args: {
    initials: 'JD',
    size: 'md',
  },
};

export const WithImage: Story = {
  args: {
    src: 'https://i.pravatar.cc/150?img=1',
    size: 'md',
  },
};

export const Small: Story = {
  args: {
    initials: 'SM',
    size: 'sm',
  },
};

export const Medium: Story = {
  args: {
    initials: 'MD',
    size: 'md',
  },
};

export const Large: Story = {
  args: {
    initials: 'LG',
    size: 'lg',
  },
};

export const ExtraLarge: Story = {
  args: {
    initials: 'XL',
    size: 'xl',
  },
};

export const AllSizes: Story = {
  render: () => ({
    template: `
      <div style="display: flex; align-items: center; gap: 16px;">
        <g-avatar size="sm" initials="SM"></g-avatar>
        <g-avatar size="md" initials="MD"></g-avatar>
        <g-avatar size="lg" initials="LG"></g-avatar>
        <g-avatar size="xl" initials="XL"></g-avatar>
      </div>
    `,
  }),
};

export const AllSizesWithImages: Story = {
  render: () => ({
    template: `
      <div style="display: flex; align-items: center; gap: 16px;">
        <g-avatar size="sm" src="https://i.pravatar.cc/150?img=1"></g-avatar>
        <g-avatar size="md" src="https://i.pravatar.cc/150?img=2"></g-avatar>
        <g-avatar size="lg" src="https://i.pravatar.cc/150?img=3"></g-avatar>
        <g-avatar size="xl" src="https://i.pravatar.cc/150?img=4"></g-avatar>
      </div>
    `,
  }),
};

export const Placeholder: Story = {
  render: () => ({
    template: `
      <div style="display: flex; align-items: center; gap: 16px;">
        <g-avatar size="sm"></g-avatar>
        <g-avatar size="md"></g-avatar>
        <g-avatar size="lg"></g-avatar>
        <g-avatar size="xl"></g-avatar>
      </div>
    `,
  }),
};

export const UserList: Story = {
  render: () => ({
    template: `
      <div style="display: flex; flex-direction: column; gap: 12px;">
        <div style="display: flex; align-items: center; gap: 12px;">
          <g-avatar size="md" src="https://i.pravatar.cc/150?img=10"></g-avatar>
          <span>John Doe - Mission Commander</span>
        </div>
        <div style="display: flex; align-items: center; gap: 12px;">
          <g-avatar size="md" initials="AS"></g-avatar>
          <span>Alice Smith - Flight Director</span>
        </div>
        <div style="display: flex; align-items: center; gap: 12px;">
          <g-avatar size="md" src="https://i.pravatar.cc/150?img=12"></g-avatar>
          <span>Bob Johnson - Systems Engineer</span>
        </div>
        <div style="display: flex; align-items: center; gap: 12px;">
          <g-avatar size="md"></g-avatar>
          <span>Unknown User - Pending Assignment</span>
        </div>
      </div>
    `,
  }),
};
