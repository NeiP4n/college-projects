"""MCP server for CodeLearn Builder API."""
import json, urllib.request, urllib.error
from mcp.server.fastmcp import FastMCP

BASE = "http://localhost:5003/api/builder"
mcp = FastMCP("codelearn")

# ── helpers ──────────────────────────────────────────────────────────────────

def _get(path: str):
    with urllib.request.urlopen(f"{BASE}{path}") as r:
        return json.loads(r.read())

def _post(path: str, body: dict):
    data = json.dumps(body, ensure_ascii=False).encode()
    req = urllib.request.Request(f"{BASE}{path}", data=data,
          headers={"Content-Type": "application/json"}, method="POST")
    with urllib.request.urlopen(req) as r:
        return json.loads(r.read())

def _put(path: str, body: dict):
    data = json.dumps(body, ensure_ascii=False).encode()
    req = urllib.request.Request(f"{BASE}{path}", data=data,
          headers={"Content-Type": "application/json"}, method="PUT")
    with urllib.request.urlopen(req) as r:
        return json.loads(r.read())

def _delete(path: str):
    req = urllib.request.Request(f"{BASE}{path}", method="DELETE")
    with urllib.request.urlopen(req) as r:
        return r.status

def _safe(fn):
    try:
        return fn()
    except urllib.error.HTTPError as e:
        return {"error": e.code, "detail": e.read().decode()[:200]}
    except Exception as e:
        return {"error": str(e)}

def _block_content(block_type: str, content: dict) -> str:
    """Serialize content dict to JSON string (double-serialized as API expects)."""
    return json.dumps(content, ensure_ascii=False)

# ── READ tools ───────────────────────────────────────────────────────────────

@mcp.tool()
def get_overview() -> str:
    """Get all modules with lessons and block counts. Use this first to understand structure."""
    modules = _safe(lambda: _get("/modules"))
    if isinstance(modules, dict) and "error" in modules:
        return json.dumps(modules)
    result = []
    for m in modules:
        lessons = m.get("lessons", [])
        result.append({
            "module_id": m["id"],
            "title": m["title"],
            "lessons": [{"id": l["id"], "title": l["title"], "order": l["order"]} for l in lessons]
        })
    return json.dumps(result, ensure_ascii=False, indent=2)

@mcp.tool()
def get_lesson_blocks(lesson_id: int) -> str:
    """Get all content blocks for a lesson. Returns id, type, order and parsed content."""
    blocks = _safe(lambda: _get(f"/lessons/{lesson_id}/blocks"))
    if isinstance(blocks, dict) and "error" in blocks:
        return json.dumps(blocks)
    result = []
    for b in blocks:
        try:
            content = json.loads(b["content"])
        except Exception:
            content = b["content"]
        result.append({"id": b["id"], "type": b["type"], "order": b["order"], "content": content})
    return json.dumps(result, ensure_ascii=False, indent=2)

@mcp.tool()
def get_stats() -> str:
    """Get platform statistics: users, lessons, completions, errors, AI usage."""
    return json.dumps(_safe(lambda: _get("/stats")), ensure_ascii=False, indent=2)

@mcp.tool()
def get_block_types() -> str:
    """Return all supported block types with their content schema."""
    types = {
        "text": {"markdown": "string — markdown content"},
        "code": {"language": "csharp|python|js|...", "title": "string", "code": "string"},
        "quiz": {"question": "string", "options": ["A","B","C","D"],
                 "correct": "int 0-based index", "explanation": "string"},
        "task": {"title": "string", "description": "string",
                 "starterCode": "string", "expectedOutput": "string", "language": "csharp"},
        "control_question": {"question": "string", "answer": "string"},
        "short_answer": {"question": "string", "answer": "string",
                         "hint": "optional string"},
        "classify": {"instruction": "string",
                     "categories": ["Cat1","Cat2"],
                     "items": [{"text":"item","category":"Cat1"}]},
        "knowledge_check": {"question": "string", "options": ["A","B","C","D"],
                            "correct": "int 0-based", "explanation": "string"},
        "challenge_task": {"title": "string", "description": "string",
                           "starterCode": "string", "expectedOutput": "string"},
    }
    return json.dumps(types, ensure_ascii=False, indent=2)

# ── WRITE: blocks ─────────────────────────────────────────────────────────────

@mcp.tool()
def add_block(lesson_id: int, block_type: str, content: dict) -> str:
    """Add a content block to a lesson.
    block_type: text|code|quiz|task|control_question|short_answer|classify|knowledge_check
    content: dict matching the block type schema (use get_block_types to see schemas)
    """
    body = {"type": block_type, "content": _block_content(block_type, content)}
    return json.dumps(_safe(lambda: _post(f"/lessons/{lesson_id}/blocks", body)),
                      ensure_ascii=False, indent=2)

@mcp.tool()
def update_block(block_id: int, block_type: str, content: dict) -> str:
    """Update (fix) an existing block by its ID.
    Use get_lesson_blocks first to get block IDs and current content.
    """
    body = {"type": block_type, "content": _block_content(block_type, content)}
    return json.dumps(_safe(lambda: _put(f"/blocks/{block_id}", body)),
                      ensure_ascii=False, indent=2)

@mcp.tool()
def delete_block(block_id: int) -> str:
    """Delete a block by ID."""
    return json.dumps(_safe(lambda: _delete(f"/blocks/{block_id}")), ensure_ascii=False)

@mcp.tool()
def reorder_blocks(lesson_id: int, block_orders: list[dict]) -> str:
    """Reorder blocks in a lesson. block_orders = [{"id": 5, "order": 0}, ...]"""
    body = {"entityType": "block", "items": block_orders}
    return json.dumps(_safe(lambda: _post("/reorder", body)), ensure_ascii=False)

# ── WRITE: lessons ────────────────────────────────────────────────────────────

@mcp.tool()
def add_lesson(module_id: int, title: str, is_locked: bool = True) -> str:
    """Create a new lesson in a module."""
    body = {"title": title, "isLocked": is_locked}
    return json.dumps(_safe(lambda: _post(f"/modules/{module_id}/lessons", body)),
                      ensure_ascii=False, indent=2)

@mcp.tool()
def update_lesson(lesson_id: int, title: str, is_locked: bool | None = None) -> str:
    """Rename a lesson or change its locked state."""
    body = {"title": title}
    if is_locked is not None:
        body["isLocked"] = is_locked
    return json.dumps(_safe(lambda: _put(f"/lessons/{lesson_id}", body)),
                      ensure_ascii=False, indent=2)

@mcp.tool()
def delete_lesson(lesson_id: int) -> str:
    """Delete a lesson (blocks become orphaned, not deleted)."""
    return json.dumps(_safe(lambda: _delete(f"/lessons/{lesson_id}")), ensure_ascii=False)

# ── WRITE: modules ────────────────────────────────────────────────────────────

@mcp.tool()
def add_module(title: str, description: str = "", course_id: int | None = None,
               is_locked: bool = True) -> str:
    """Create a new module (topic group). course_id optional."""
    body = {"title": title, "description": description, "isLocked": is_locked}
    if course_id is not None:
        body["courseId"] = course_id
    return json.dumps(_safe(lambda: _post("/modules", body)), ensure_ascii=False, indent=2)

@mcp.tool()
def update_module(module_id: int, title: str, description: str | None = None,
                  is_locked: bool | None = None) -> str:
    """Rename or update a module."""
    body = {"title": title}
    if description is not None:
        body["description"] = description
    if is_locked is not None:
        body["isLocked"] = is_locked
    return json.dumps(_safe(lambda: _put(f"/modules/{module_id}", body)),
                      ensure_ascii=False, indent=2)

# ── COMPOUND: fill lesson ─────────────────────────────────────────────────────

@mcp.tool()
def fill_lesson(lesson_id: int, blocks: list[dict]) -> str:
    """Add multiple blocks to a lesson in one call.
    blocks = [{"type": "text", "content": {...}}, {"type": "quiz", "content": {...}}, ...]
    Returns summary of added blocks.
    """
    results = []
    for b in blocks:
        body = {"type": b["type"], "content": _block_content(b["type"], b["content"])}
        res = _safe(lambda bdy=body: _post(f"/lessons/{lesson_id}/blocks", bdy))
        results.append({"type": b["type"], "ok": "error" not in res,
                        "id": res.get("id"), "error": res.get("error")})
    ok = sum(1 for r in results if r["ok"])
    return json.dumps({"added": ok, "total": len(blocks), "results": results},
                      ensure_ascii=False, indent=2)

if __name__ == "__main__":
    mcp.run(transport="stdio")
