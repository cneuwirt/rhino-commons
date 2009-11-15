properties { 
  $base_dir  = resolve-path .
  $lib_dir = "$base_dir\SharedLibs"
  $build_dir = "$base_dir\build" 
  $buildartifacts_dir = "$build_dir\" 
  $sln_file = "$base_dir\Rhino.Commons-vs2008.sln" 
  $version = "2.0.0.0"
  $humanReadableversion = "2.0"
  $tools_dir = "$base_dir\Tools"
  $release_dir = "$base_dir\Release"
  $uploadCategory = "Rhino-Commons"
  $uploadScript = "C:\Builds\Upload\PublishBuild.build"
} 

task default -depends Release

task Clean { 
  remove-item -force -recurse $buildartifacts_dir -ErrorAction SilentlyContinue 
  remove-item -force -recurse $release_dir -ErrorAction SilentlyContinue 
} 

task Init -depends Clean { 
	. .\psake_ext.ps1
	Generate-Assembly-Info `
		-file "$base_dir\Rhino.Commons\Properties\AssemblyInfo.cs" `
		-clsCompliant "true" `
		-title "Rhino.Commons for Microsoft .NET Framework 3.5" `
		-description "Rhino Commons - Support and extension for many advance usecases" `
		-company "Rhino Tools Project" `
		-product "Rhino.Commons" `
		-copyright "Rhino Tools Project, original author or authors" `
		-version $version `
		
	Generate-Assembly-Info `
		-file "$base_dir\Rhino.Commons.ActiveRecord\Properties\AssemblyInfo.cs" `
		-clsCompliant "true" `
		-title "Rhino.Commons.ActiveRecord for Microsoft .NET Framework 3.5" `
		-description "Rhino Commons - Support and extension for many advance usecases" `
		-company "Rhino Tools Project" `
		-product "Rhino.Commons.ActiveRecord" `
		-copyright "Rhino Tools Project, original author or authors" `
		-version $version `

	Generate-Assembly-Info `
		-file "$base_dir\Rhino.Commons.Binsor\Properties\AssemblyInfo.cs" `
		-clsCompliant "true" `
		-title "Rhino.Commons.Binsor for Microsoft .NET Framework 3.5" `
		-description "Rhino Commons - Support and extension for many advance usecases" `
		-company "Rhino Tools Project" `
		-product "Rhino.Commons.Binsor" `
		-copyright "Rhino Tools Project, original author or authors" `
		-version $version `

	Generate-Assembly-Info `
		-file "$base_dir\Rhino.Commons.Clr\Properties\AssemblyInfo.cs" `
		-clsCompliant "true" `
		-title "Rhino.Commons.Clr for Microsoft .NET Framework 3.5" `
		-description "Rhino Commons Clr - The missing bits from the BCL" `
		-company "Rhino Tools Project" `
		-product "Rhino.Commons.Clr" `
		-copyright "Rhino Tools Project, original author or authors" `
		-version $version `

	Generate-Assembly-Info `
		-file "$base_dir\Rhino.Commons.NHibernate\Properties\AssemblyInfo.cs" `
		-clsCompliant "true" `
		-title "Rhino.Commons.NHibernate for Microsoft .NET Framework 3.5" `
		-description "Rhino Commons - Support and extension for many advance usecases" `
		-company "Rhino Tools Project" `
		-product "Rhino.Commons.NHibernate" `
		-copyright "Rhino Tools Project, original author or authors" `
		-version $version `
			
	new-item $release_dir -itemType directory 
	new-item $buildartifacts_dir -itemType directory 
	cp $tools_dir\MbUnit\*.* $build_dir
    cp $lib_dir\SqlCE\*.* $build_dir
} 

task Compile -depends Init { 
  exec msbuild "/p:OutDir=""$buildartifacts_dir "" $sln_file"
} 

task Test -depends Compile {
  $old = pwd
  cd $build_dir
  exec ".\MbUnit.Cons.exe" "$build_dir\Rhino.Commons.Test.dll"
  cd $old
}

task Release -depends Test {
	& $tools_dir\zip.exe -9 -A -j `
		$release_dir\Rhino.Commons-$humanReadableversion-Build-$env:ccnetnumericlabel.zip `
		$build_dir\Rhino.Commons.dll `
		$build_dir\Rhino.Commons.xml `
		$build_dir\Rhino.Commons.ActiveRecord.dll `
		$build_dir\Rhino.Commons.ActiveRecord.xml `
		$build_dir\Rhino.Commons.Binsor.dll `
		$build_dir\Rhino.Commons.Binsor.xml `
		$build_dir\Rhino.Commons.Clr.dll `
		$build_dir\Rhino.Commons.Clr.xml `
		$build_dir\Rhino.Commons.NHibernate.dll `
		$build_dir\Rhino.Commons.NHibernate.xml `
		license.txt
	if ($lastExitCode -ne 0) {
        throw "Error: Failed to execute ZIP command"
    }
}

task Upload -depend Release {
	if (Test-Path $uploadScript ) {
		$log = git log -n 1 --oneline		
		msbuild $uploadScript /p:Category=$uploadCategory "/p:Comment=$log" "/p:File=$release_dir\Rhino.Commons-$humanReadableversion-Build-$env:ccnetnumericlabel.zip"
		
		if ($lastExitCode -ne 0) {
			throw "Error: Failed to publish build"
		}
	}
	else {
		Write-Host "could not find upload script $uploadScript, skipping upload"
	}
}