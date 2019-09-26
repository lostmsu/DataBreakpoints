namespace DataBreakpoints {
    using System;
    using System.Collections.Generic;

    class DisposableCollection: IDisposable {
        readonly List<IDisposable> disposables = new List<IDisposable>();
        public void Add(IDisposable disposable) {
            if (disposable == null) throw new ArgumentNullException(nameof(disposable));
            this.disposables.Add(disposable);
        }

        public void Dispose() {
            foreach (var disposable in this.disposables) {
                disposable.Dispose();
            }
            this.disposables.Clear();
        }
    }
}
