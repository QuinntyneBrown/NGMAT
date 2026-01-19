import { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MissionCard, Mission } from './mission-card';

const meta: Meta<MissionCard> = {
  title: 'Components/Content/MissionCard',
  component: MissionCard,
  tags: ['autodocs'],
  decorators: [
    moduleMetadata({
      imports: [MatCardModule, MatIconModule],
    }),
  ],
  argTypes: {
    mission: {
      control: 'object',
    },
  },
};

export default meta;
type Story = StoryObj<MissionCard>;

export const Planning: Story = {
  args: {
    mission: {
      name: 'Artemis IV',
      status: 'planning',
      description: 'Fourth crewed mission to establish permanent lunar presence and test deep space habitation systems.',
      launchDate: '2025-09-15',
    } as Mission,
  },
};

export const Active: Story = {
  args: {
    mission: {
      name: 'Mars Perseverance',
      status: 'active',
      description: 'Ongoing exploration of Jezero Crater, collecting samples and searching for signs of ancient microbial life.',
      launchDate: '2020-07-30',
    } as Mission,
  },
};

export const Completed: Story = {
  args: {
    mission: {
      name: 'Apollo 11',
      status: 'completed',
      description: 'Historic first crewed lunar landing mission that successfully placed humans on the Moon.',
      launchDate: '1969-07-16',
    } as Mission,
  },
};

export const Failed: Story = {
  args: {
    mission: {
      name: 'Mars Climate Orbiter',
      status: 'failed',
      description: 'Mars orbiter mission that was lost due to navigation errors during orbital insertion.',
      launchDate: '1998-12-11',
    } as Mission,
  },
};

export const OnHold: Story = {
  args: {
    mission: {
      name: 'Europa Clipper',
      status: 'on-hold',
      description: 'Mission to investigate Europa\'s ice shell and subsurface ocean for potential habitability.',
      launchDate: '2024-10-10',
    } as Mission,
  },
};

export const MissionGrid: Story = {
  render: () => ({
    template: `
      <div style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 16px; max-width: 800px;">
        <g-mission-card
          [mission]="{
            name: 'Artemis III',
            status: 'planning',
            description: 'First crewed lunar landing since Apollo 17, targeting the lunar south pole.',
            launchDate: '2025-12-01'
          }"
        ></g-mission-card>
        <g-mission-card
          [mission]="{
            name: 'Voyager 1',
            status: 'active',
            description: 'Interstellar mission continuing to transmit data from beyond the heliosphere.',
            launchDate: '1977-09-05'
          }"
        ></g-mission-card>
        <g-mission-card
          [mission]="{
            name: 'Cassini-Huygens',
            status: 'completed',
            description: 'Saturn orbiter and Titan lander that revolutionized our understanding of the Saturn system.',
            launchDate: '1997-10-15'
          }"
        ></g-mission-card>
        <g-mission-card
          [mission]="{
            name: 'JWST Servicing',
            status: 'on-hold',
            description: 'Proposed servicing mission to extend James Webb Space Telescope operational lifetime.',
            launchDate: '2030-01-01'
          }"
        ></g-mission-card>
      </div>
    `,
  }),
};
