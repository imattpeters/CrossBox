using System;
//using Cirrious.MvvmCross.IoC;
using Cirrious.MvvmCross.ViewModels;
using CrossBox.Core.Services;
using CrossBox.Core;

namespace CrossBox.Core.ViewModels {
	public abstract class CrossBoxViewModel : MvxViewModel {

		//public MvxOpenNetCfContainer Container {
			//get { return MvxOpenNetCfContainer.Current; }
		//}

		public void ReportError( Exception exception ) {
			
			Mvx.Resolve<IErrorReporter>().ReportError( exception );
		}

	}
}
