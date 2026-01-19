import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';

export interface ChartTab {
  label: string;
  value: string;
}

@Component({
  selector: 'g-chart-tabs',
  standalone: true,
  imports: [CommonModule, MatTabsModule],
  templateUrl: './chart-tabs.html',
  styleUrls: ['./chart-tabs.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChartTabs {
  @Input() tabs: ChartTab[] = [];
  @Input() selectedTab = '';

  @Output() tabChange = new EventEmitter<string>();

  get selectedIndex(): number {
    const index = this.tabs.findIndex(tab => tab.value === this.selectedTab);
    return index >= 0 ? index : 0;
  }

  onTabChange(index: number): void {
    if (this.tabs[index]) {
      this.tabChange.emit(this.tabs[index].value);
    }
  }
}
