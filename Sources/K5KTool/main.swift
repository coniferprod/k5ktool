import K5KToolCore

let tool = K5KTool()

do {
    try tool.run()
} catch {
   print("Error: \(error)")
}
