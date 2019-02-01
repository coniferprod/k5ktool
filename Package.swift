// swift-tools-version:4.2
// The swift-tools-version declares the minimum version of Swift required to build this package.

import PackageDescription

let package = Package(
    name: "K5KTool",
    dependencies: [
        // Dependencies declare other packages that this package depends on.
        // .package(url: /* package url */, from: "1.0.0"),
        .package(
            url: "https://github.com/johnsundell/files.git",
            from: "1.0.0"
        ),
        .package(
            url: "https://github.com/uraimo/Bitter.git",
            from: "3.1.1"
        )
    ],
    targets: [
        // Targets are the basic building blocks of a package. A target can define a module or a test suite.
        // Targets can depend on other targets in this package, and on products in packages which this package depends on.
        .target(
            name: "K5KTool",
            dependencies: ["K5KToolCore"]
        ),
        .target(
            name: "K5KToolCore",
            dependencies: ["Files", "Bitter"]
        ),
        .testTarget(
            name: "K5KToolTests",
            dependencies: ["K5KToolCore", "Files", "Bitter"]
        )
    ]
)
