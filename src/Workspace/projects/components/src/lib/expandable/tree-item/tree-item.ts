import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

export interface TreeItemNode {
  label: string;
  icon?: string;
  expanded?: boolean;
  children?: TreeItemNode[];
}

@Component({
  selector: 'g-tree-item',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './tree-item.html',
  styleUrls: ['./tree-item.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TreeItem {
  @Input() label = '';
  @Input() icon = '';
  @Input() children: TreeItemNode[] = [];
  @Input() expanded = false;
  @Input() level = 0;

  @Output() expandedChange = new EventEmitter<boolean>();
  @Output() itemClick = new EventEmitter<void>();

  get hasChildren(): boolean {
    return this.children && this.children.length > 0;
  }

  get indentPadding(): string {
    return `${this.level * 16}px`;
  }

  toggle(event: Event): void {
    event.stopPropagation();
    if (this.hasChildren) {
      this.expanded = !this.expanded;
      this.expandedChange.emit(this.expanded);
    }
  }

  onItemClick(): void {
    this.itemClick.emit();
  }

  onChildExpandedChange(index: number, expanded: boolean): void {
    if (this.children[index]) {
      this.children[index].expanded = expanded;
    }
  }
}
